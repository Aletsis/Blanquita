using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Domain.Repositories;
using Blanquita.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class ConfiguracionServiceTests
{
    private readonly Mock<ISystemConfigurationRepository> _repositoryMock;
    private readonly Mock<IFileSystemService> _fileSystemServiceMock;
    private readonly Mock<IAppConfigurationManager> _configManagerMock;
    private readonly Mock<ILogger<ConfiguracionService>> _loggerMock;
    private readonly ConfiguracionService _service;

    public ConfiguracionServiceTests()
    {
        _repositoryMock = new Mock<ISystemConfigurationRepository>();
        _fileSystemServiceMock = new Mock<IFileSystemService>();
        _configManagerMock = new Mock<IAppConfigurationManager>();
        _loggerMock = new Mock<ILogger<ConfiguracionService>>();
        
        ConfiguracionService.ClearCache();

        _service = new ConfiguracionService(
            _repositoryMock.Object, 
            _fileSystemServiceMock.Object,
            _configManagerMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ShouldCreateNew_WhenNotExists()
    {
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemConfiguration?)null);

        var result = await _service.ObtenerConfiguracionAsync();

        Assert.NotNull(result);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<SystemConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ShouldReturnExisting_WhenExists()
    {
        var config = new SystemConfiguration 
        { 
            Pos10041Path = "test/path",
            PrinterName = "TestPrinter"
        };
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var result = await _service.ObtenerConfiguracionAsync();

        Assert.NotNull(result);
        Assert.Equal("test/path", result.Pos10041Path);
        Assert.Equal("TestPrinter", result.PrinterName);
    }

    [Fact]
    public async Task GuardarConfiguracionAsync_ShouldThrow_WhenValidationFails()
    {
        var dto = new ConfiguracionDto { Pos10041Path = "invalid" };

        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

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
            Pos10008Path = "valid/path8",
            Pos10010Path = "valid/path9",
            PrinterName = "Printer1"
        };

        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemConfiguration());

        await _service.GuardarConfiguracionAsync(dto);

        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SystemConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidarConfiguracionAsync_ShouldReturnErrors_WhenPathsEmpty()
    {
        var dto = new ConfiguracionDto();
        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

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
            Mgw10011Path = "path7",
            Pos10008Path = "path8",
            Pos10010Path = "path9"
        };

        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

        var result = await _service.ValidarConfiguracionAsync(dto);

        Assert.True(result.EsValido);
        Assert.Empty(result.Errores);
    }

    [Fact]
    public void ValidarRutaArchivo_ShouldDelegateToFileSystemService()
    {
        _fileSystemServiceMock.Setup(x => x.FileExists("test/path")).Returns(true);

        var result = _service.ValidarRutaArchivo("test/path");

        Assert.True(result);
        _fileSystemServiceMock.Verify(x => x.FileExists("test/path"), Times.Once);
    }

    [Fact]
    public void ObtenerNombreArchivo_ShouldReturnCorrectName()
    {
        Assert.Equal("POS10041.DBF", _service.ObtenerNombreArchivo(TipoArchivoDbf.Pos10041));
    }

    [Fact]
    public async Task RestablecerConfiguracionAsync_ShouldResetToDefaults()
    {
        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemConfiguration());

        await _service.RestablecerConfiguracionAsync();

        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SystemConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
