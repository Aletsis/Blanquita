using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.DiagnoseFoxProFile;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class DiagnoseFoxProFileQueryHandlerTests
{
    private readonly Mock<IFoxProDiagnosticService> _mockService;
    private readonly Mock<ILogger<DiagnoseFoxProFileQueryHandler>> _mockLogger;
    private readonly DiagnoseFoxProFileQueryHandler _handler;

    public DiagnoseFoxProFileQueryHandlerTests()
    {
        _mockService = new Mock<IFoxProDiagnosticService>();
        _mockLogger = new Mock<ILogger<DiagnoseFoxProFileQueryHandler>>();
        _handler = new DiagnoseFoxProFileQueryHandler(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidFile_ReturnsSuccessfulDiagnosis()
    {
        // Arrange
        var path = "C:\\test\\file.dbf";
        var expectedColumns = new List<string> { "COL1", "COL2" };
        var expectedResult = new DiagnosticoResultado
        {
            RutaCompleta = path,
            NombreArchivo = "file.dbf",
            ArchivoExiste = true,
            ConexionExitosa = true,
            Exitoso = true,
            NumeroRegistros = 100,
            Columnas = new List<ColumnaInfo>
            {
                new() { Nombre = "COL1", TipoDato = "String", Tamaño = 50 },
                new() { Nombre = "COL2", TipoDato = "Decimal", Tamaño = 10 }
            },
            ColumnasEsperadas = expectedColumns,
            Logs = new List<string> { "Diagnóstico exitoso" },
            Errores = new List<string>(),
            Advertencias = new List<string>(),
            TiempoEjecucion = TimeSpan.FromMilliseconds(100)
        };

        _mockService
            .Setup(s => s.DiagnoseFileAsync(path, expectedColumns, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var query = new DiagnoseFoxProFileQuery(path, expectedColumns);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Exitoso);
        Assert.True(result.ArchivoExiste);
        Assert.True(result.ConexionExitosa);
        Assert.Equal(100, result.NumeroRegistros);
        Assert.Equal(2, result.Columnas.Count);
        Assert.Empty(result.Errores);

        _mockService.Verify(s => s.DiagnoseFileAsync(path, expectedColumns, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FileNotFound_ReturnsFailedDiagnosis()
    {
        // Arrange
        var path = "C:\\test\\nonexistent.dbf";
        var expectedResult = new DiagnosticoResultado
        {
            RutaCompleta = path,
            NombreArchivo = "nonexistent.dbf",
            ArchivoExiste = false,
            ConexionExitosa = false,
            Exitoso = false,
            Errores = new List<string> { "El archivo no existe." },
            Logs = new List<string> { "Error: El archivo no existe en la ruta especificada." },
            Advertencias = new List<string>(),
            Columnas = new List<ColumnaInfo>(),
            ColumnasEsperadas = new List<string>(),
            TiempoEjecucion = TimeSpan.FromMilliseconds(50)
        };

        _mockService
            .Setup(s => s.DiagnoseFileAsync(path, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var query = new DiagnoseFoxProFileQuery(path, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Exitoso);
        Assert.False(result.ArchivoExiste);
        Assert.False(result.ConexionExitosa);
        Assert.Single(result.Errores);
        Assert.Contains("El archivo no existe.", result.Errores);

        _mockService.Verify(s => s.DiagnoseFileAsync(path, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingExpectedColumns_ReturnsWarnings()
    {
        // Arrange
        var path = "C:\\test\\file.dbf";
        var expectedColumns = new List<string> { "COL1", "COL2", "COL3" };
        var expectedResult = new DiagnosticoResultado
        {
            RutaCompleta = path,
            NombreArchivo = "file.dbf",
            ArchivoExiste = true,
            ConexionExitosa = true,
            Exitoso = true,
            NumeroRegistros = 50,
            Columnas = new List<ColumnaInfo>
            {
                new() { Nombre = "COL1", TipoDato = "String", Tamaño = 50 }
            },
            ColumnasEsperadas = expectedColumns,
            Logs = new List<string> { "Advertencia: Faltan 2 columnas esperadas" },
            Errores = new List<string>(),
            Advertencias = new List<string> { "Faltan las siguientes columnas esperadas: COL2, COL3" },
            TiempoEjecucion = TimeSpan.FromMilliseconds(100)
        };

        _mockService
            .Setup(s => s.DiagnoseFileAsync(path, expectedColumns, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var query = new DiagnoseFoxProFileQuery(path, expectedColumns);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Exitoso);
        Assert.Single(result.Advertencias);
        Assert.Contains("COL2", result.Advertencias[0]);
        Assert.Contains("COL3", result.Advertencias[0]);

        _mockService.Verify(s => s.DiagnoseFileAsync(path, expectedColumns, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_PropagatesException()
    {
        // Arrange
        var path = "C:\\test\\file.dbf";
        var expectedException = new InvalidOperationException("Service error");

        _mockService
            .Setup(s => s.DiagnoseFileAsync(path, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new DiagnoseFoxProFileQuery(path, null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }
}
