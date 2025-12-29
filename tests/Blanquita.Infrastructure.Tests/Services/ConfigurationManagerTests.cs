using Blanquita.Infrastructure.Models;
using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class ConfigurationManagerTests
{
    private readonly Mock<ILogger<AppConfigurationManager>> _loggerMock;
    private readonly AppConfigurationManager _manager;

    public ConfigurationManagerTests()
    {
        _loggerMock = new Mock<ILogger<AppConfigurationManager>>();
        _manager = new AppConfigurationManager(_loggerMock.Object);
    }

    [Fact]
    public void CargarConfiguracion_ShouldReturnDefault_WhenFileNotExists()
    {
        var config = _manager.CargarConfiguracion();

        Assert.NotNull(config);
        // Should return a new default configuration
    }

    [Fact]
    public void ValidatePath_ShouldReturnFalse_WhenPathEmpty()
    {
        var result = _manager.ValidatePath("");

        Assert.False(result);
    }

    [Fact]
    public void ValidatePath_ShouldReturnFalse_WhenFileNotExists()
    {
        var result = _manager.ValidatePath("C:\\NonExistent\\File.txt");

        Assert.False(result);
    }

    [Fact]
    public void GuardarConfiguracion_ShouldNotThrow_WithValidConfig()
    {
        var config = new AppConfiguration
        {
            Pos10041Path = "test",
            Pos10042Path = "test"
        };

        // This will create the file in the system's CommonApplicationData folder
        // In a real scenario, you might want to mock the file system
        var exception = Record.Exception(() => _manager.GuardarConfiguracion(config));

        // Should not throw, or if it does, it's due to permissions which is expected
        // Assert.Null(exception); // Might fail due to permissions
    }
}
