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
    public async Task GetSalesReportByFiltersAsync_SoportaHoraEnFormatoFoxProHHMMSSSinDosPuntos()
    {
        var rowTarde = Row(Hoy, "COH", "20");
        rowTarde["CHORA"] = "143025"; // 14:30:25 en POS10008 (TimeSpan.TryParse lo interpretaba como 143,025 días)

        var rowMananaSinCeroInicial = Row(Hoy, "COH", "19");
        rowMananaSinCeroInicial["CHORA"] = "91530"; // 09:15:30

        var rowDecimalFoxPro = Row(Hoy, "COH", "21");
        rowDecimalFoxPro["CHORA"] = "175959.00"; // 17:59:59

        var rowFueraDeRango = Row(Hoy, "COH", "22");
        rowFueraDeRango["CHORA"] = "231000"; // 23:10:00

        SetupRows(rowFueraDeRango, rowDecimalFoxPro, rowTarde, rowMananaSinCeroInicial);

        var result = (await _repository.GetSalesReportByFiltersAsync(
            Hoy,
            Hoy,
            new TimeSpan(7, 0, 0),
            new TimeSpan(22, 0, 0),
            new[] { "COH" })).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("COH-19", result[0].Folio);
        Assert.Equal("09:15:30", result[0].Hora);
        Assert.Equal("COH-20", result[1].Folio);
        Assert.Equal("14:30:25", result[1].Hora);
        Assert.Equal("COH-21", result[2].Folio);
        Assert.Equal("17:59:59", result[2].Hora);
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

    [Fact]
    public void FoxProReverseDataReader_LeeRegistrosEnOrdenInversoYSaltaBorrados()
    {
        var dbfPath = Path.GetTempFileName();
        try
        {
            WriteMinimalDbf(dbfPath, new[]
            {
                (Deleted: false, IdDoc: "100", ProdId: 10, Unidades: 2.50m),
                (Deleted: true,  IdDoc: "100", ProdId: 99, Unidades: 9.00m),
                (Deleted: false, IdDoc: "101", ProdId: 20, Unidades: 4.00m)
            });

            using var reader = new FoxProReverseDataReader(dbfPath);

            Assert.True(reader.Read());
            Assert.Equal("101", reader.GetStringSafe("CIDDOCUM01"));
            Assert.Equal(20, reader.GetInt32Safe("CIDPRODU01"));
            Assert.Equal(4.00m, reader.GetDecimalSafe("CUNIDADES"));

            Assert.True(reader.Read());
            Assert.Equal("100", reader.GetStringSafe("CIDDOCUM01"));
            Assert.Equal(10, reader.GetInt32Safe("CIDPRODU01"));
            Assert.Equal(2.50m, reader.GetDecimalSafe("CUNIDADES"));

            Assert.False(reader.Read());
        }
        finally
        {
            if (File.Exists(dbfPath)) File.Delete(dbfPath);
        }
    }

    private static void WriteMinimalDbf(string path, IReadOnlyList<(bool Deleted, string IdDoc, int ProdId, decimal Unidades)> records)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs, System.Text.Encoding.ASCII);

        // 3 columnas: CIDDOCUM01 (C,10), CIDPRODU01 (N,8,0), CUNIDADES (N,10,2)
        // RecordLength = 1 (status) + 10 + 8 + 10 = 29 bytes
        // HeaderLength = 32 + (3 * 32) + 1 = 129 bytes
        bw.Write((byte)0x03); // dBASE III
        bw.Write(new byte[] { 26, 11, 9 }); // YY, MM, DD
        bw.Write(records.Count);
        bw.Write((short)129); // HeaderLength
        bw.Write((short)29);  // RecordLength
        bw.Write(new byte[20]);

        WriteColumnDescriptor(bw, "CIDDOCUM01", 'C', 10, 0);
        WriteColumnDescriptor(bw, "CIDPRODU01", 'N', 8, 0);
        WriteColumnDescriptor(bw, "CUNIDADES", 'N', 10, 2);
        bw.Write((byte)0x0D); // Terminator

        foreach (var r in records)
        {
            bw.Write((byte)(r.Deleted ? 0x2A : 0x20));
            bw.Write(System.Text.Encoding.ASCII.GetBytes(r.IdDoc.PadRight(10)[..10]));
            bw.Write(System.Text.Encoding.ASCII.GetBytes(r.ProdId.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(8)[..8]));
            bw.Write(System.Text.Encoding.ASCII.GetBytes(r.Unidades.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).PadLeft(10)[..10]));
        }

        bw.Write((byte)0x1A); // EOF
    }

    private static void WriteColumnDescriptor(BinaryWriter bw, string name, char type, byte length, byte decimals)
    {
        var nameBytes = new byte[11];
        var ascii = System.Text.Encoding.ASCII.GetBytes(name);
        Array.Copy(ascii, nameBytes, Math.Min(ascii.Length, 10));
        bw.Write(nameBytes);
        bw.Write((byte)type);
        bw.Write(0); // Offset
        bw.Write(length);
        bw.Write(decimals);
        bw.Write(new byte[14]);
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
