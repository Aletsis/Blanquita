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
        if (OperatingSystem.IsWindows())
        {
            // Skip assertion on Windows
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var expectedFilePath = Path.Combine(tempDir, "pg_dump");
            File.WriteAllText(expectedFilePath, string.Empty);

            _configMock.Setup(c => c["DatabaseBackup:PostgresBinPath"]).Returns(tempDir);

            var methodInfo = typeof(DatabaseBackupService).GetMethod("FindPgToolPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            var result = (string)methodInfo.Invoke(_service, new object[] { "pg_dump" })!;

            Assert.False(result.EndsWith(".exe"), "On Linux, the tool path should not end with .exe");
            Assert.Equal(expectedFilePath, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void FindPgToolPath_ShouldAppendExe_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Skip on non-Windows
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var expectedFilePath = Path.Combine(tempDir, "pg_dump.exe");
            File.WriteAllText(expectedFilePath, string.Empty);

            _configMock.Setup(c => c["DatabaseBackup:PostgresBinPath"]).Returns(tempDir);

            var methodInfo = typeof(DatabaseBackupService).GetMethod("FindPgToolPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            var result = (string)methodInfo.Invoke(_service, new object[] { "pg_dump" })!;

            Assert.True(result.EndsWith(".exe", StringComparison.OrdinalIgnoreCase), "On Windows, the tool path should end with .exe");
            Assert.Equal(expectedFilePath, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void FindPgToolPath_WhenToolNotFound_ShouldThrowFileNotFoundException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            _configMock.Setup(c => c["DatabaseBackup:PostgresBinPath"]).Returns(tempDir);

            var methodInfo = typeof(DatabaseBackupService).GetMethod("FindPgToolPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                methodInfo.Invoke(_service, new object[] { "non_existent_pg_tool_xyz_123" })
            );

            Assert.IsType<FileNotFoundException>(ex.InnerException);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
