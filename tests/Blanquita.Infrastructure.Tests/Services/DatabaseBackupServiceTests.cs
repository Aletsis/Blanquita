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
    public void FindPgToolPath_ShouldUseConfiguredBinPath_WithPlatformExtension()
    {
        // Herramienta falsa en una carpeta temporal: la prueba no depende de tener PostgreSQL instalado
        var binDir = Path.Combine(Path.GetTempPath(), $"pgbin_{Guid.NewGuid():N}");
        Directory.CreateDirectory(binDir);
        var toolName = OperatingSystem.IsWindows() ? "pg_dump.exe" : "pg_dump";
        var toolPath = Path.Combine(binDir, toolName);
        File.WriteAllText(toolPath, string.Empty);

        try
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["DatabaseBackup:PostgresBinPath"]).Returns(binDir);
            var service = new DatabaseBackupService(config.Object, _loggerMock.Object);

            var result = service.FindPgToolPath("pg_dump");

            Assert.Equal(toolPath, result);
            Assert.Equal(OperatingSystem.IsWindows(), result.EndsWith(".exe"));
        }
        finally
        {
            Directory.Delete(binDir, recursive: true);
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

            Assert.Throws<FileNotFoundException>(() =>
                _service.FindPgToolPath("non_existent_pg_tool_xyz_123")
            );
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private static (DatabaseBackupService Service, string Directory) CreateServiceWithTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "blanquita-backup-tests-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=user;Password=pass",
                ["DatabaseBackup:BackupDirectory"] = directory
            })
            .Build();

        return (new DatabaseBackupService(config, new Mock<ILogger<DatabaseBackupService>>().Object), directory);
    }

    [Theory]
    [InlineData("../../etc/respaldo.backup", "respaldo.backup")]
    [InlineData("C:\\Users\\x\\respaldo.backup", "respaldo.backup")]
    [InlineData("res<pal>do.backup", "res_pal_do.backup")]
    public void SanitizeFileName_ShouldRemovePathsAndInvalidCharacters(string input, string expected)
    {
        Assert.Equal(expected, DatabaseBackupService.SanitizeFileName(input));
    }

    [Fact]
    public void SanitizeFileName_ShouldGenerateName_WhenOnlyExtensionIsProvided()
    {
        var result = DatabaseBackupService.SanitizeFileName(".backup");

        Assert.StartsWith("respaldo_subido_", result);
        Assert.EndsWith(".backup", result);
    }

    [Fact]
    public void QuoteIdentifier_ShouldEscapeDoubleQuotes()
    {
        Assert.Equal("\"mi\"\"base\"", DatabaseBackupService.QuoteIdentifier("mi\"base"));
    }

    [Fact]
    public void BuildDatabaseName_ShouldRespectPostgresIdentifierLimit()
    {
        var name = DatabaseBackupService.BuildDatabaseName(new string('a', 80), "_restore_20261030120000");

        Assert.True(name.Length <= 63);
        Assert.EndsWith("_restore_20261030120000", name);
    }

    [Fact]
    public async Task SaveUploadedBackupAsync_ShouldNotOverwriteExistingFile()
    {
        var (service, directory) = CreateServiceWithTempDirectory();
        try
        {
            var first = await service.SaveUploadedBackupAsync(new MemoryStream(new byte[] { 1 }), "respaldo.backup");
            var second = await service.SaveUploadedBackupAsync(new MemoryStream(new byte[] { 2 }), "respaldo.backup");

            Assert.Equal(Path.Combine(directory, "respaldo.backup"), first);
            Assert.Equal(Path.Combine(directory, "respaldo_1.backup"), second);
            Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(first));
            Assert.Equal(new byte[] { 2 }, File.ReadAllBytes(second));
        }
        finally
        {
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveUploadedBackupAsync_ShouldKeepFileInsideBackupDirectory_WhenNameContainsPath()
    {
        var (service, directory) = CreateServiceWithTempDirectory();
        try
        {
            var saved = await service.SaveUploadedBackupAsync(new MemoryStream(new byte[] { 1 }), "../../fuera.backup");

            Assert.Equal(Path.Combine(directory, "fuera.backup"), saved);
        }
        finally
        {
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveUploadedBackupAsync_ShouldReject_NonBackupFiles()
    {
        var (service, directory) = CreateServiceWithTempDirectory();
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SaveUploadedBackupAsync(new MemoryStream(new byte[] { 1 }), "script.sql"));
        }
        finally
        {
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAndRestore_ShouldReject_FilesOutsideBackupDirectory()
    {
        var (service, directory) = CreateServiceWithTempDirectory();
        var outside = Path.Combine(Path.GetTempPath(), "otro-" + Guid.NewGuid().ToString("N") + ".backup");
        File.WriteAllBytes(outside, new byte[] { 1 });
        try
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteBackupAsync(outside));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RestoreBackupAsync(outside));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.DeleteBackupAsync(Path.Combine(directory, "..", Path.GetFileName(outside))));

            Assert.True(File.Exists(outside));
        }
        finally
        {
            File.Delete(outside);
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }
}
