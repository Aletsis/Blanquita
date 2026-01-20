using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class ConfiguracionServiceTests
{
    private readonly BlanquitaDbContext _context;
    private readonly Mock<IAppConfigurationManager> _configManagerMock;
    private readonly Mock<ILogger<ConfiguracionService>> _loggerMock;
    private readonly ConfiguracionService _service;

    public ConfiguracionServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BlanquitaDbContext(options);
        _configManagerMock = new Mock<IAppConfigurationManager>();
        _loggerMock = new Mock<ILogger<ConfiguracionService>>();
        
        _service = new ConfiguracionService(_context, _configManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ShouldCreateNew_WhenNotExists()
    {
        var result = await _service.ObtenerConfiguracionAsync();

        Assert.NotNull(result);
        var inDb = await _context.SystemConfigurations.FirstOrDefaultAsync();
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ShouldReturnExisting_WhenExists()
    {
        var config = new SystemConfiguration 
        { 
            Pos10041Path = "test/path",
            PrinterName = "TestPrinter"
        };
        _context.SystemConfigurations.Add(config);
        await _context.SaveChangesAsync();

        var result = await _service.ObtenerConfiguracionAsync();

        Assert.NotNull(result);
        Assert.Equal("test/path", result.Pos10041Path);
        Assert.Equal("TestPrinter", result.PrinterName);
    }

    [Fact]
    public async Task GuardarConfiguracionAsync_ShouldThrow_WhenValidationFails()
    {
        var dto = new ConfiguracionDto
        {
            Pos10041Path = "", // Empty path should fail validation
            Pos10042Path = "",
            Mgw10008Path = "",
            Mgw10005Path = "",
            Mgw10045Path = "",
            Mgw10002Path = "",
            Mgw10011Path = ""
        };

        _configManagerMock.Setup(x => x.ValidatePath(It.IsAny<string>())).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.GuardarConfiguracionAsync(dto));
    }

    [Fact]
    public async Task GuardarConfiguracionAsync_ShouldSave_WhenValid()
    {
        var dto = new ConfiguracionDto
        {
            Pos10041Path = "valid/path1",
            Pos10042Path = "valid/path2",
            Mgw10008Path = "valid/path3",
            Mgw10005Path = "valid/path4",
            Mgw10045Path = "valid/path5",
            Mgw10002Path = "valid/path6",
            Mgw10011Path = "valid/path7",
            PrinterName = "Printer1"
        };

        _configManagerMock.Setup(x => x.ValidatePath(It.IsAny<string>())).Returns(true);

        await _service.GuardarConfiguracionAsync(dto);

        var saved = await _context.SystemConfigurations.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("valid/path1", saved.Pos10041Path);
        Assert.Equal("valid/path6", saved.Mgw10002Path);
        Assert.Equal("Printer1", saved.PrinterName);
    }

    [Fact]
    public async Task ValidarConfiguracionAsync_ShouldReturnErrors_WhenPathsEmpty()
    {
        var dto = new ConfiguracionDto();
        _configManagerMock.Setup(x => x.ValidatePath(It.IsAny<string>())).Returns(false);

        var result = await _service.ValidarConfiguracionAsync(dto);

        Assert.False(result.EsValido);
        Assert.NotEmpty(result.Errores);
    }

    [Fact]
    public async Task ValidarConfiguracionAsync_ShouldReturnValid_WhenAllPathsExist()
    {
        var dto = new ConfiguracionDto
        {
            Pos10041Path = "path1",
            Pos10042Path = "path2",
            Mgw10008Path = "path3",
            Mgw10005Path = "path4",
            Mgw10045Path = "path5",
            Mgw10002Path = "path6",
            Mgw10011Path = "path7"
        };

        _configManagerMock.Setup(x => x.ValidatePath(It.IsAny<string>())).Returns(true);

        var result = await _service.ValidarConfiguracionAsync(dto);

        Assert.True(result.EsValido);
        Assert.Empty(result.Errores);
    }

    [Fact]
    public void ValidarRutaArchivo_ShouldDelegateToConfigManager()
    {
        _configManagerMock.Setup(x => x.ValidatePath("test/path")).Returns(true);

        var result = _service.ValidarRutaArchivo("test/path");

        Assert.True(result);
        _configManagerMock.Verify(x => x.ValidatePath("test/path"), Times.Once);
    }

    [Fact]
    public void ObtenerNombreArchivo_ShouldReturnCorrectName()
    {
        Assert.Equal("POS10041.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Pos10041));
        Assert.Equal("POS10042.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Pos10042));
        Assert.Equal("MGW10008.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Mgw10008));
        Assert.Equal("MGW10005.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Mgw10005));
        Assert.Equal("MGW10045.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Mgw10045));
        Assert.Equal("MGW10002.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Mgw10002));
        Assert.Equal("MGW10011.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Mgw10011));
    }

    [Fact]
    public async Task RestablecerConfiguracionAsync_ShouldResetToDefaults()
    {
        // First create a config with values
        var config = new SystemConfiguration { Pos10041Path = "old/path" };
        _context.SystemConfigurations.Add(config);
        await _context.SaveChangesAsync();

        _configManagerMock.Setup(x => x.ValidatePath(It.IsAny<string>())).Returns(false);

        // This should throw because default config has empty paths
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.RestablecerConfiguracionAsync());
    }
}
