using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class CashCollectionServiceTests
{
    private readonly Mock<ICashCollectionRepository> _repositoryMock;
    private readonly Mock<ILogger<CashCollectionService>> _loggerMock;
    private readonly CashCollectionService _service;

    public CashCollectionServiceTests()
    {
        _repositoryMock = new Mock<ICashCollectionRepository>();
        _loggerMock = new Mock<ILogger<CashCollectionService>>();
        _service = new CashCollectionService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        var entity = CashCollection.Create(1, 0, 0, 0, 0, 0, "Reg1", "Cashier1", "Sup1", 100);
        // Using reflection to set ID since it's protected/private set usually
        typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, 5);

        _repositoryMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal(1000m, result.TotalAmount); // 1000 * 1
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepo_AndReturnDto()
    {
        var dto = new CreateCashCollectionDto
        {
            Thousands = 1,
            CashRegisterName = "Reg1",
            CashierName = "Cashier1",
            SupervisorName = "Sup1",
            IsForCashCut = false
        };

        _repositoryMock.Setup(x => x.GetNextFolioAsync("Reg1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(101);

        CashCollection? capturedEntity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<CashCollection>(), It.IsAny<CancellationToken>()))
            .Callback<CashCollection, CancellationToken>((c, t) => capturedEntity = c)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(101, result.Folio);
        Assert.Equal(1000m, result.TotalAmount);

        Assert.NotNull(capturedEntity);
        Assert.Equal(101, capturedEntity.Folio);
        Assert.Equal("Reg1", capturedEntity.CashRegisterName);
    }

    [Fact]
    public async Task MarkAsCutAsync_ShouldUpdateEntities()
    {
        var col1 = CashCollection.Create(1, 0, 0, 0, 0, 0, "Reg1", "C", "S", 1);
        var col2 = CashCollection.Create(1, 0, 0, 0, 0, 0, "Reg1", "C", "S", 2);
        var collections = new List<CashCollection> { col1, col2 };

        _repositoryMock.Setup(x => x.GetPendingCollectionsByRegisterAsync("Reg1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(collections);

        await _service.MarkAsCutAsync("Reg1");

        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<CashCollection>(c => c.IsForCashCut), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.True(col1.IsForCashCut);
        Assert.True(col2.IsForCashCut);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterResults()
    {
        var c1 = CashCollection.Create(1, 0, 0, 0, 0, 0, "Reg1", "User1", "Sup1", 1); // 1000
        var c2 = CashCollection.Create(0, 1, 0, 0, 0, 0, "Reg2", "User2", "Sup1", 2); // 500
        
        var all = new List<CashCollection> { c1, c2 };
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(all);

        // Filter by Register
        var request = new SearchCashCollectionRequest { CashRegisterName = "Reg1" };
        var results = await _service.SearchAsync(request);

        Assert.Single(results);
        Assert.Equal("Reg1", results.First().CashRegisterName);
    }
}
