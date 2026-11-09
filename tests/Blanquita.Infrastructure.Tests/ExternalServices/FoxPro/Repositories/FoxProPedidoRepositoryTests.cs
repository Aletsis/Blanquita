using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.ExternalServices.FoxPro.Repositories;

public class FoxProPedidoRepositoryTests : IDisposable
{
    private static readonly DateTime Hoy = new(2026, 11, 8);

    private readonly Mock<IConfiguracionService> _mockConfigService = new();
    private readonly Mock<IFoxProReaderFactory> _mockReaderFactory = new();
    private readonly Mock<IClientCatalogRepository> _mockClientRepository = new();
    private readonly Mock<IProductCatalogRepository> _mockProductRepository = new();
    private readonly FoxProPedidoRepository _repository;
    private readonly string _tempFilePath;

    public FoxProPedidoRepositoryTests()
    {
        _tempFilePath = Path.GetTempFileName();

        _mockConfigService
            .Setup(s => s.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto
            {
                Pos10008Path = _tempFilePath,
                Pos10010Path = _tempFilePath
            });

        _mockClientRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ClientSearchDto>());

        _repository = new FoxProPedidoRepository(
            _mockConfigService.Object,
            _mockReaderFactory.Object,
            _mockClientRepository.Object,
            _mockProductRepository.Object,
            Mock.Of<ILogger<FoxProPedidoRepository>>());
    }

    public void Dispose()
    {
        try { File.Delete(_tempFilePath); } catch { }
    }

    private void SetupRows(params Dictionary<string, object>[] rows)
    {
        _mockReaderFactory
            .Setup(f => f.CreateReverseReader(_tempFilePath))
            .Returns(() => new FakeFoxProReader(rows));
    }

    private static Dictionary<string, object> Row(DateTime fecha, string serie, string folio, string texto01 = "", string texto02 = "") => new()
    {
        ["CFECHA"] = fecha,
        ["CSERIEDO01"] = serie,
        ["CFOLIO"] = folio,
        ["CIDDOCUM01"] = $"{serie}{folio}",
        ["CTEXTOEX01"] = texto01,
        ["CTEXTOEX02"] = texto02,
        ["CTOTAL"] = 100m,
        ["CIDCLIEN01"] = 1,
    };

    [Fact]
    public async Task SearchPedidosAsync_SoloDevuelveLaFechaExacta()
    {
        // Lectura en reversa: más recientes primero
        SetupRows(
            Row(Hoy.AddDays(1), "PA", "10"),
            Row(Hoy, "PA", "9"),
            Row(Hoy.AddDays(-1), "PA", "8"),
            Row(Hoy.AddDays(-30), "PA", "7"));

        var result = (await _repository.SearchPedidosAsync(Hoy, new[] { "PA" })).ToList();

        var pedido = Assert.Single(result);
        Assert.Equal("PA-9", pedido.Folio);
        Assert.Equal("PA", pedido.Serie);
        Assert.Equal("9", pedido.NumeroFolio);
    }

    [Fact]
    public async Task SearchPedidosAsync_FiltraPorSerie()
    {
        SetupRows(
            Row(Hoy, "PA", "1"),
            Row(Hoy, "PB", "2"));

        var result = (await _repository.SearchPedidosAsync(Hoy, new[] { "pb" })).ToList();

        Assert.Equal("PB-2", Assert.Single(result).Folio);
    }

    [Theory]
    [InlineData("R01")]
    [InlineData("r01")]
    [InlineData(" R01 ")]
    [InlineData("R1")]
    [InlineData("1")]
    [InlineData("01")]
    public async Task SearchRutasAsync_RutaCoincide(string filtro)
    {
        SetupRows(
            Row(Hoy, "PA", "1", "R01 1343"),
            Row(Hoy, "PA", "2", "R02 1343"));

        var result = (await _repository.SearchRutasAsync(Hoy, new[] { "PA" }, ruta: filtro)).ToList();

        Assert.Equal("PA-1", Assert.Single(result).Folio);
    }

    [Theory]
    [InlineData("R10")]
    [InlineData("X01")]
    [InlineData("10")]
    public async Task SearchRutasAsync_RutaNoCoincide(string filtro)
    {
        SetupRows(Row(Hoy, "PA", "1", "R01 1343"));

        var result = await _repository.SearchRutasAsync(Hoy, new[] { "PA" }, ruta: filtro);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("1343", true)]
    [InlineData("01343", true)]
    [InlineData("134", false)]
    public async Task SearchRutasAsync_FiltraPorNominaToleraCeros(string nomina, bool esperado)
    {
        SetupRows(Row(Hoy, "PA", "1", "R01 01343"));

        var result = await _repository.SearchRutasAsync(Hoy, new[] { "PA" }, repartidorNomina: nomina);

        Assert.Equal(esperado, result.Any());
    }

    [Fact]
    public async Task GetSalesReportByFiltersAsync_FiltraPorRangoDeFechasYHora()
    {
        var rowDentroRango = Row(Hoy, "COH", "10");
        rowDentroRango["CHORA"] = "14:30:45";

        var rowHoraLimiteMinuto = Row(Hoy, "COH", "11");
        rowHoraLimiteMinuto["CHORA"] = "18:00:35";

        var rowFueraHora = Row(Hoy, "COH", "12");
        rowFueraHora["CHORA"] = "19:00:00";

        var rowAntesDeFechaInicio = Row(Hoy.AddDays(-5), "COH", "9");
        rowAntesDeFechaInicio["CHORA"] = "12:00:00";

        SetupRows(rowFueraHora, rowHoraLimiteMinuto, rowDentroRango, rowAntesDeFechaInicio);

        var result = (await _repository.GetSalesReportByFiltersAsync(
            Hoy,
            Hoy,
            new TimeSpan(10, 0, 0),
            new TimeSpan(18, 0, 0),
            new[] { "COH" })).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Folio == "COH-10");
        Assert.Contains(result, p => p.Folio == "COH-11");
    }

    [Fact]
    public async Task GetPedidoItemsAsync_NoCortaPorRegistrosConIdMenorIntercalados()
    {
        // Simulamos más de 5,000 registros con CIDDOCUM01 menor intercalados con IDs mayores
        // antes de llegar a la partida del documento buscado ("10000").
        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 6000; i++)
        {
            rows.Add(new Dictionary<string, object>
            {
                // Cada 100 registros entra uno con ID mayor (como pasa al sincronizar varias cajas/sucursales)
                ["CIDDOCUM01"] = i % 100 == 0 ? "15000" : "5000",
                ["CIDPRODU01"] = 1,
                ["CUNIDADES"] = 1m,
                ["CPRECIO"] = 10m,
                ["CNETO"] = 10m
            });
        }

        rows.Add(new Dictionary<string, object>
        {
            ["CIDDOCUM01"] = "10000",
            ["CIDPRODU01"] = 42,
            ["CUNIDADES"] = 3m,
            ["CPRECIO"] = 25m,
            ["CNETO"] = 75m
        });

        _mockProductRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new ProductSearchDto { Id = 42, Code = "P42", Name = "Producto 42" } });

        SetupRows(rows.ToArray());

        var items = (await _repository.GetPedidoItemsAsync("10000")).ToList();

        var item = Assert.Single(items);
        Assert.Equal("P42", item.Codigo);
        Assert.Equal(3m, item.Cantidad);
        Assert.Equal(75m, item.Subtotal);
    }

    /// <summary>
    /// Lector en memoria que expone filas como columnas por nombre.
    /// </summary>
    private sealed class FakeFoxProReader : IFoxProDataReader
    {
        private readonly IReadOnlyList<Dictionary<string, object>> _rows;
        private readonly List<string> _columns;
        private int _index = -1;

        public FakeFoxProReader(IReadOnlyList<Dictionary<string, object>> rows)
        {
            _rows = rows;
            _columns = rows.SelectMany(r => r.Keys).Distinct().ToList();
        }

        private object? Current(int ordinal) =>
            _rows[_index].TryGetValue(_columns[ordinal], out var v) ? v : null;

        public bool Read() => ++_index < _rows.Count;
        public int GetOrdinal(string name)
        {
            var i = _columns.IndexOf(name);
            if (i < 0) throw new IndexOutOfRangeException(name);
            return i;
        }
        public bool IsDBNull(int ordinal) => Current(ordinal) is null;
        public object GetValue(int ordinal) => Current(ordinal)!;
        public string GetString(int ordinal) => Current(ordinal)?.ToString() ?? string.Empty;
        public decimal GetDecimal(int ordinal) => Convert.ToDecimal(Current(ordinal));
        public int GetInt32(int ordinal) => Convert.ToInt32(Current(ordinal));
        public DateTime GetDateTime(int ordinal) => (DateTime)Current(ordinal)!;
        public void Dispose() { }
    }
}
