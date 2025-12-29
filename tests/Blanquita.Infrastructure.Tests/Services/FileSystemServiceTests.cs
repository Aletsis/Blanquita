using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class FileSystemServiceTests
{
    private readonly Mock<ILogger<FileSystemService>> _loggerMock;
    private readonly FileSystemService _service;

    public FileSystemServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileSystemService>>();
        _service = new FileSystemService(_loggerMock.Object);
    }

    [Fact]
    public async Task GetAvailableDrivesAsync_ShouldReturnDrives()
    {
        var drives = await _service.GetAvailableDrivesAsync();

        Assert.NotNull(drives);
        // Should have at least C: drive on Windows
        Assert.NotEmpty(drives);
    }

    [Fact]
    public async Task GetDirectoriesAsync_ShouldReturnEmpty_WhenPathEmpty()
    {
        var result = await _service.GetDirectoriesAsync("");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDirectoriesAsync_ShouldReturnEmpty_WhenPathInvalid()
    {
        var result = await _service.GetDirectoriesAsync("C:\\NonExistentPath12345");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDbfFilesAsync_ShouldReturnEmpty_WhenPathEmpty()
    {
        var result = await _service.GetDbfFilesAsync("");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDbfFilesAsync_ShouldReturnEmpty_WhenPathInvalid()
    {
        var result = await _service.GetDbfFilesAsync("C:\\NonExistentPath12345");

        Assert.Empty(result);
    }

    [Fact]
    public void FileExists_ShouldReturnFalse_WhenPathEmpty()
    {
        var result = _service.FileExists("");

        Assert.False(result);
    }

    [Fact]
    public void FileExists_ShouldReturnFalse_WhenFileNotExists()
    {
        var result = _service.FileExists("C:\\NonExistent\\File.txt");

        Assert.False(result);
    }

    [Fact]
    public void ValidateFileName_ShouldReturnFalse_WhenPathEmpty()
    {
        var result = _service.ValidateFileName("", "test.dbf");

        Assert.False(result);
    }

    [Fact]
    public void ValidateFileName_ShouldReturnTrue_WhenNamesMatch()
    {
        var result = _service.ValidateFileName("C:\\Path\\test.dbf", "test.dbf");

        Assert.True(result);
    }

    [Fact]
    public void ValidateFileName_ShouldReturnTrue_WhenNamesMatchCaseInsensitive()
    {
        var result = _service.ValidateFileName("C:\\Path\\TEST.DBF", "test.dbf");

        Assert.True(result);
    }

    [Fact]
    public void ValidateFileName_ShouldReturnFalse_WhenNamesDontMatch()
    {
        var result = _service.ValidateFileName("C:\\Path\\test.dbf", "other.dbf");

        Assert.False(result);
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnNull_WhenPathEmpty()
    {
        var result = _service.GetParentDirectory("");

        Assert.Null(result);
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnParent_WhenPathValid()
    {
        var result = _service.GetParentDirectory("C:\\Path\\SubPath");

        Assert.NotNull(result);
        Assert.Contains("Path", result);
    }

    [Fact]
    public void GetFileName_ShouldReturnEmpty_WhenPathEmpty()
    {
        var result = _service.GetFileName("");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFileName_ShouldReturnFileName_WhenPathValid()
    {
        var result = _service.GetFileName("C:\\Path\\test.dbf");

        Assert.Equal("test.dbf", result);
    }

    [Fact]
    public void HasDirectoryAccess_ShouldReturnFalse_WhenPathEmpty()
    {
        var result = _service.HasDirectoryAccess("");

        Assert.False(result);
    }

    [Fact]
    public void HasDirectoryAccess_ShouldReturnFalse_WhenPathNotExists()
    {
        var result = _service.HasDirectoryAccess("C:\\NonExistentPath12345");

        Assert.False(result);
    }
}
