using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;

namespace Blanquita.Infrastructure.Tests.Services;

public class DatabaseBackupServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<DatabaseBackupService>> _loggerMock;
    private readonly DatabaseBackupService _service;

    public DatabaseBackupServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DatabaseBackupService>>();
        
        // Mock connection string to avoid errors
        _configMock.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns("Host=localhost;Database=test;Username=user;Password=pass");
        
        _service = new DatabaseBackupService(_configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void FindPgToolPath_ShouldNotAppendExe_OnLinux()
    {
        // This test assumes running on Linux environment.
        // If the platform is not Windows, FindPgToolPath("pg_dump") should search for "pg_dump".
        
        // Arrange
        // We know /usr/bin/pg_dump exists on this environment.
        
        // Act
        // This is a private method, but we can't test private methods directly easily.
        // We can test by calling a public method that uses it, but it might throw if tools are missing or DB connection fails.
        // Actually, we can use reflection to test the private method.
        
        var methodInfo = typeof(DatabaseBackupService).GetMethod("FindPgToolPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = (string)methodInfo.Invoke(_service, new object[] { "pg_dump" });
        
        // Assert
        Assert.False(result.EndsWith(".exe"), "On Linux, the tool path should not end with .exe");
        Assert.Equal("/usr/bin/pg_dump", result);
    }
}
