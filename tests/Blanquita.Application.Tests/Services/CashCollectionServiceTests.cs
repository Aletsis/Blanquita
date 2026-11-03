using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Application.Services;
using Blanquita.Application.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class CashCollectionServiceTests
{
    private readonly Mock<ICashCollectionRepository> _repositoryMock;
    private readonly Mock<ILogger<CashCollectionService>> _loggerMock;
    private readonly PassThroughUnitOfWork _unitOfWork;
    private readonly CashCollectionService _service;

    public CashCollectionServiceTests()
    {
        _repositoryMock = new Mock<ICashCollectionRepository>();
        _loggerMock = new Mock<ILogger<CashCollectionService>>();
        _unitOfWork = new PassThroughUnitOfWork();
        _service = new CashCollectionService(_repositoryMock.Object, _loggerMock.Object, _unitOfWork);
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
    public async Task SearchAsync_ShouldSendFiltersToRepository()
    {
        CashCollectionSearchCriteria? criteria = null;
        _repositoryMock.Setup(x => x.SearchAsync(It.IsAny<CashCollectionSearchCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<CashCollectionSearchCriteria, CancellationToken>((c, _) => criteria = c)
            .ReturnsAsync(new List<CashCollection> { CashCollection.Create(1, 0, 0, 0, 0, 0, "Reg1", "User1", "Sup1", 1) });

        var request = new SearchCashCollectionRequest { CashRegisterName = "Reg1", IsCut = false, Page = 2, PageSize = 10 };
        var results = await _service.SearchAsync(request);

        Assert.Single(results);
        Assert.NotNull(criteria);
        Assert.Equal("Reg1", criteria!.CashRegisterName);
        Assert.False(criteria.IsForCashCut);
        Assert.Equal(10, criteria.Skip);
        Assert.Equal(10, criteria.Take);
        Assert.Null(criteria.FromUtc);
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldIncludeEveningCollections_WhenFilteringByLocalDay()
    {
        // Recolección hecha hoy a las 20:00 hora local, almacenada en UTC (como la devuelve Npgsql).
        // En zonas con offset negativo (México) cae en el día UTC siguiente: el rango debe cubrirla.
        var eveningUtc = DateTime.SpecifyKind(DateTime.Today.AddHours(20), DateTimeKind.Local).ToUniversalTime();

        CashCollectionSearchCriteria? criteria = null;
        _repositoryMock.Setup(x => x.SearchAsync(It.IsAny<CashCollectionSearchCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<CashCollectionSearchCriteria, CancellationToken>((c, _) => criteria = c)
            .ReturnsAsync(new List<CashCollection>());

        var request = new SearchCashCollectionRequest { FechaInicio = DateTime.Today, FechaFin = DateTime.Today };
        await _service.SearchAsync(request);

        Assert.NotNull(criteria);
        Assert.InRange(eveningUtc, criteria!.FromUtc!.Value, criteria.ToUtc!.Value);
    }

    [Fact]
    public async Task CreateAsync_ShouldAssignFolioUnderRegisterLock()
    {
        _repositoryMock.Setup(x => x.GetNextFolioAsync("Reg1", It.IsAny<CancellationToken>())).ReturnsAsync(7);

        var dto = new CreateCashCollectionDto { Thousands = 1, CashRegisterName = "Reg1", CashierName = "C", SupervisorName = "S" };
        var result = await _service.CreateAsync(dto);

        Assert.Equal(7, result.Folio);
        Assert.Equal(new[] { CashCollectionService.GetRegisterLockKey("Reg1") }, _unitOfWork.LockKeys);
    }
}
