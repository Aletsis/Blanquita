using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class LabelDesignServiceTests
{
    private readonly BlanquitaDbContext _context;
    private readonly Mock<ILogger<LabelDesignService>> _loggerMock;
    private readonly LabelDesignService _service;

    public LabelDesignServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _context = new BlanquitaDbContext(options);
        _loggerMock = new Mock<ILogger<LabelDesignService>>();
        
        _service = new LabelDesignService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddDesignToDb()
    {
        var dto = new LabelDesignDto
        {
            Name = "New Design",
            WidthInMm = 50,
            HeightInMm = 20,
            IsDefault = true
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        
        var inDb = await _context.LabelDesigns.FirstOrDefaultAsync(x => x.Name == "New Design");
        Assert.NotNull(inDb);
        Assert.True(inDb.IsDefault);
    }

    [Fact]
    public async Task CreateAsync_ShouldThow_IfNameExists()
    {
        _context.LabelDesigns.Add(LabelDesign.Create("Duplicate", 50, 20));
        await _context.SaveChangesAsync();

        var dto = new LabelDesignDto { Name = "Duplicate" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task SetAsDefaultAsync_ShouldUnsetOthers()
    {
        var d1 = LabelDesign.Create("D1", 50, 20, isDefault: true);
        var d2 = LabelDesign.Create("D2", 50, 20, isDefault: false);
        _context.LabelDesigns.AddRange(d1, d2);
        await _context.SaveChangesAsync();

        // Act: Set D2 as default
        await _service.SetAsDefaultAsync(d2.Id);

        // Reload to check changes
        var d1Db = await _context.LabelDesigns.FindAsync(d1.Id);
        var d2Db = await _context.LabelDesigns.FindAsync(d2.Id);

        Assert.False(d1Db!.IsDefault);
        Assert.True(d2Db!.IsDefault);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFromDb()
    {
        var d1 = LabelDesign.Create("D1", 50, 20);
        _context.LabelDesigns.Add(d1);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(d1.Id);

        var count = await _context.LabelDesigns.CountAsync();
        Assert.Equal(0, count);
    }
}
