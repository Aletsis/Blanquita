using Blanquita.Application.Interfaces;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.ExternalServices.FoxPro.Repositories;

public class FoxProProductRepositoryTests : IDisposable
{
    private readonly Mock<IConfiguracionService> _mockConfigService;
    private readonly Mock<IFoxProReaderFactory> _mockReaderFactory;
    private readonly Mock<ILogger<FoxProProductRepository>> _mockLogger;
    private readonly Mock<IFoxProDataReader> _mockReader;
    private readonly FoxProProductRepository _repository;
    private readonly string _tempFilePath;

    public FoxProProductRepositoryTests()
    {
        _mockConfigService = new Mock<IConfiguracionService>();
        _mockReaderFactory = new Mock<IFoxProReaderFactory>();
        _mockLogger = new Mock<ILogger<FoxProProductRepository>>();
        _mockReader = new Mock<IFoxProDataReader>();

        _repository = new FoxProProductRepository(
            _mockConfigService.Object,
            _mockReaderFactory.Object,
            _mockLogger.Object);

        // Crear archivo temporal real para pasar la validación File.Exists
        _tempFilePath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            try { File.Delete(_tempFilePath); } catch { }
        }
    }

    [Fact]
    public async Task GetByCodeAsync_ProductFound_ReturnsProduct()
    {
        // Arrange
        var code = "TEST001";
        
        // Mock Config con ruta real temporal
        var config = new Application.DTOs.ConfiguracionDto { Mgw10005Path = _tempFilePath };
        _mockConfigService.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Mock Reader Factory
        _mockReaderFactory.Setup(f => f.CreateReader(_tempFilePath)).Returns(_mockReader.Object);

        // Mock Reader Behavior
        // Simular que encuentra 1 registro y luego termina
        _mockReader.SetupSequence(r => r.Read())
            .Returns(true)  // Primer registro
            .Returns(false); // Fin

        // Mock GetOrdinal para mapear nombres de columnas a IDs
        _mockReader.Setup(r => r.GetOrdinal("CCODIGOP01")).Returns(0);
        _mockReader.Setup(r => r.GetOrdinal("CNOMBREP01")).Returns(1);
        _mockReader.Setup(r => r.GetOrdinal("CPRECIO1")).Returns(2);
        _mockReader.Setup(r => r.GetOrdinal("CIMPUESTO1")).Returns(3);

        // Mock GetValue para devolver los valores en el orden esperado
        // El repositorio usa extensiones que llaman a GetValue y luego convierten
        // Pero las extensiones son ESTÁTICAS en DbfReaderExtensions (ahora FoxProReaderExtensions wrapper?)
        // Espera, las extensiones IFoxProReaderExtensions llaman a GetValue(ordinal) y GetOrdinal(name)
        
        _mockReader.Setup(r => r.GetValue(0)).Returns(code);
        _mockReader.Setup(r => r.GetValue(1)).Returns("Producto Test");
        _mockReader.Setup(r => r.GetValue(2)).Returns(100.50m);
        _mockReader.Setup(r => r.GetValue(3)).Returns(16.0m);

        // Add DateTime mock if needed by mapper? NO, mapper only uses String and Decimal for Product

        // Act
        var result = await _repository.GetByCodeAsync(code, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
        Assert.Equal("Producto Test", result.Name);
        Assert.Equal(100.50m, result.BasePrice);
        Assert.Equal(16.0m, result.TaxRate);
    }

    [Fact]
    public async Task GetByCodeAsync_ProductNotFound_ReturnsNull()
    {
        // Arrange
        var code = "NONEXISTENT";
        
        var config = new Application.DTOs.ConfiguracionDto { Mgw10005Path = _tempFilePath };
        _mockConfigService.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        _mockReaderFactory.Setup(f => f.CreateReader(_tempFilePath)).Returns(_mockReader.Object);

        // Simular archivo vacío o sin coincidencias
        _mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);

        _mockReader.Setup(r => r.GetOrdinal("CCODIGOP01")).Returns(0);
        _mockReader.Setup(r => r.GetValue(0)).Returns("OTHER_CODE"); // Código diferente

        // Act
        var result = await _repository.GetByCodeAsync(code, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
