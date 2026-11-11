using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.Export;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.ExternalServices.Export;

public class ExportServiceTests
{
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _loggerMock = new Mock<ILogger<ExportService>>();
        _service = new ExportService(_loggerMock.Object);
    }

    public class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    [Fact]
    public async Task ExportToExcelAsync_ShouldReturnEmpty_WhenNoData()
    {
        var data = new List<TestData>();

        var result = await _service.ExportToExcelAsync(data);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExportToExcelAsync_ShouldGenerateExcel_WhenDataExists()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Test1", Amount = 100.50m, Date = DateTime.Now },
            new TestData { Id = 2, Name = "Test2", Amount = 200.75m, Date = DateTime.Now }
        };

        var result = await _service.ExportToExcelAsync(data, "TestSheet");

        Assert.NotEmpty(result);
        // Excel files start with PK (0x50 0x4B) - ZIP format
        Assert.Equal(0x50, result[0]);
        Assert.Equal(0x4B, result[1]);
    }

    [Fact]
    public async Task ExportToPdfAsync_ShouldReturnEmpty_WhenNoData()
    {
        var data = new List<TestData>();

        var result = await _service.ExportToPdfAsync(data);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExportToPdfAsync_ShouldGeneratePdf_WhenDataExists()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Test1", Amount = 100.50m, Date = DateTime.Now },
            new TestData { Id = 2, Name = "Test2", Amount = 200.75m, Date = DateTime.Now }
        };

        var result = await _service.ExportToPdfAsync(data, "Test Report");

        Assert.NotEmpty(result);
        // PDF files start with %PDF
        Assert.Equal(0x25, result[0]); // %
        Assert.Equal(0x50, result[1]); // P
        Assert.Equal(0x44, result[2]); // D
        Assert.Equal(0x46, result[3]); // F
    }

    [Fact]
    public async Task ExportToExcelAsync_ShouldHandleDifferentDataTypes()
    {
        var data = new List<TestData>
        {
            new TestData 
            { 
                Id = 999, 
                Name = "Complex Test", 
                Amount = 12345.67m, 
                Date = new DateTime(2024, 1, 1, 10, 30, 0) 
            }
        };

        var result = await _service.ExportToExcelAsync(data);

        Assert.NotEmpty(result);
        Assert.True(result.Length > 100); // Should have substantial content
    }

    [Fact]
    public async Task ExportToPdfAsync_ShouldHandleMultipleRows()
    {
        var data = Enumerable.Range(1, 10).Select(i => new TestData
        {
            Id = i,
            Name = $"Item {i}",
            Amount = i * 10.5m,
            Date = DateTime.Now.AddDays(-i)
        }).ToList();

        var result = await _service.ExportToPdfAsync(data, "Multi-Row Report");

        Assert.NotEmpty(result);
        // Should generate a valid PDF
        Assert.Equal(0x25, result[0]);
        Assert.Equal(0x50, result[1]);
        Assert.Equal(0x44, result[2]);
        Assert.Equal(0x46, result[3]);
    }

    [Fact]
    public async Task ExportConciliacionesPdfAsync_ShouldGenerateValidPdf_WithoutSalidas()
    {
        var branch = new BranchDto { Id = 1, Code = "SUC1", Name = "Matriz", Direccion = "Calle 1" };
        var data = new List<ConciliacionCorteDto>
        {
            new()
            {
                Caja = "Caja 1",
                Cajero = "Ana García",
                TotalRecolecciones = 1000m,
                EfectivoEntregado = 500m,
                SalidasEfectivo = 0m,
                TotalEfectivo = 1500m,
                TotalEntregado = 1500m,
                TotalEsperado = 1500m,
                Diferencia = 0m
            }
        };

        var result = await _service.ExportConciliacionesPdfAsync(data, branch, DateTime.Today);

        Assert.NotEmpty(result);
        Assert.Equal(0x25, result[0]); // %
        Assert.Equal(0x50, result[1]); // P
        Assert.Equal(0x44, result[2]); // D
        Assert.Equal(0x46, result[3]); // F
    }

    [Fact]
    public async Task ExportConciliacionesPdfAsync_ShouldGenerateValidPdf_WithSalidasDetail()
    {
        var branch = new BranchDto { Id = 1, Code = "SUC1", Name = "Matriz", Direccion = "Calle 1" };
        var conciliacionesSinSalidas = new List<ConciliacionCorteDto>
        {
            new()
            {
                Caja = "Caja 1",
                Cajero = "Ana García",
                TotalRecolecciones = 1000m,
                EfectivoEntregado = 500m,
                SalidasEfectivo = 0m,
                TotalEfectivo = 1500m,
                TotalEntregado = 1500m,
                TotalEsperado = 1500m,
                Diferencia = 0m
            }
        };

        var conciliacionesConSalidas = new List<ConciliacionCorteDto>
        {
            new()
            {
                Caja = "Caja 1",
                Cajero = "Ana García",
                TotalRecolecciones = 1000m,
                EfectivoEntregado = 0m,
                SalidasEfectivo = 500m,
                TotalEfectivo = 1500m,
                TotalEntregado = 1500m,
                TotalEsperado = 1500m,
                Diferencia = 0m,
                Salidas = new List<ConciliacionSalidaEfectivoDto>
                {
                    new() { Motivo = "Compra de bolsas", QuienAutoriza = "Juan Pérez", Monto = 350m },
                    new() { Motivo = "Pago de garrafones", QuienAutoriza = "Juan Pérez", Monto = 150m }
                }
            }
        };

        var pdfSinSalidas = await _service.ExportConciliacionesPdfAsync(conciliacionesSinSalidas, branch, DateTime.Today);
        var pdfConSalidas = await _service.ExportConciliacionesPdfAsync(conciliacionesConSalidas, branch, DateTime.Today);

        Assert.NotEmpty(pdfConSalidas);
        Assert.Equal(0x25, pdfConSalidas[0]);
        Assert.Equal(0x50, pdfConSalidas[1]);
        Assert.Equal(0x44, pdfConSalidas[2]);
        Assert.Equal(0x46, pdfConSalidas[3]);
        Assert.True(pdfConSalidas.Length > pdfSinSalidas.Length);
    }
}
