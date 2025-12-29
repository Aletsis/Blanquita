using Blanquita.Infrastructure.Services;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class BranchServiceTests
{
    private readonly BranchService _service;

    public BranchServiceTests()
    {
        _service = new BranchService();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBranches()
    {
        var result = await _service.GetAllAsync();
        
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        Assert.Contains(result, b => b.Name == "Himno Nacional");
        Assert.Contains(result, b => b.Name == "Pozos");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBranch_WhenExists()
    {
        var result = await _service.GetByIdAsync(6);
        
        Assert.NotNull(result);
        Assert.Equal("Himno Nacional", result.Name);
        Assert.Equal(6, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(999);
        
        Assert.Null(result);
    }
}
