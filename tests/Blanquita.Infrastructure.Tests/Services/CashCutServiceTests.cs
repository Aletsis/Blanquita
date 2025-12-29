using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class CashCutServiceTests
{
    private readonly Mock<ICashCutRepository> _repositoryMock;
    private readonly Mock<ILogger<CashCutService>> _loggerMock;
    private readonly Mock<ICashCollectionService> _collectionServiceMock;
    private readonly Mock<ISupervisorService> _supervisorServiceMock;
    private readonly Mock<ICashierService> _cashierServiceMock;
    private readonly Mock<ICashRegisterService> _registerServiceMock;
    private readonly Mock<IBranchService> _branchServiceMock;
    private readonly CashCutService _service;

    public CashCutServiceTests()
    {
        _repositoryMock = new Mock<ICashCutRepository>();
        _loggerMock = new Mock<ILogger<CashCutService>>();
        _collectionServiceMock = new Mock<ICashCollectionService>();
        _supervisorServiceMock = new Mock<ISupervisorService>();
        _cashierServiceMock = new Mock<ICashierService>();
        _registerServiceMock = new Mock<ICashRegisterService>();
        _branchServiceMock = new Mock<IBranchService>();

        _service = new CashCutService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _collectionServiceMock.Object,
            _supervisorServiceMock.Object,
            _cashierServiceMock.Object,
            _registerServiceMock.Object,
            _branchServiceMock.Object
        );
    }

    [Fact]
    public async Task ProcessCashCutAsync_ShouldAggregateCollectionsAndCreateCut()
    {
        // Arrange
        var request = new ProcessCashCutRequest
        {
            CashRegisterId = 1,
            SupervisorId = 10,
            CashierId = 20,
            TotalSlips = 10000m,
            TotalCards = 500m
        };

        // Mock Entities
        var registerDto = new CashRegisterDto { Id = 1, Name = "Reg1" };
        var supervisorDto = new SupervisorDto { Id = 10, Name = "Super1", BranchId = 99 };
        var cashierDto = new CashierDto { Id = 20, Name = "Cashier1" };
        var branchDto = new BranchDto { Id = 99, Name = "Branch1" };

        _registerServiceMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(registerDto);
        _supervisorServiceMock.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(supervisorDto);
        _cashierServiceMock.Setup(x => x.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(cashierDto);
        _branchServiceMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(branchDto);

        // Mock Collections
        var col1 = new CashCollectionDto { Thousands = 1, FiveHundreds = 2, TwoHundreds = 0, Hundreds = 0, Fifties = 0, Twenties = 0 };
        var col2 = new CashCollectionDto { Thousands = 2, FiveHundreds = 0, TwoHundreds = 0, Hundreds = 0, Fifties = 0, Twenties = 0 };
        var collections = new List<CashCollectionDto> { col1, col2 };

        _collectionServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCashCollectionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(collections);

        // Act
        var result = await _service.ProcessCashCutAsync(request);

        // Assert
        // Verify MarkAsCut was called
        _collectionServiceMock.Verify(x => x.MarkAsCutAsync("Reg1", It.IsAny<CancellationToken>()), Times.Once);

        // Verify Repository.AddAsync was called with correct sums
        // Total Thousands: 1 + 2 = 3
        // Total 500s: 2 + 0 = 2
        _repositoryMock.Verify(x => x.AddAsync(It.Is<CashCut>(c => 
            c.Totals.TotalThousands == 3 &&
            c.Totals.TotalFiveHundreds == 2 &&
            c.CashRegisterName == "Reg1" &&
            c.SupervisorName == "Super1" &&
            c.CashierName == "Cashier1" &&
            c.BranchName == "Branch1"
        ), It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalThousands);
    }

    [Fact]
    public async Task ProcessCashCutAsync_ShouldThrow_IfDepsNotFound()
    {
        var request = new ProcessCashCutRequest { CashRegisterId = 1 };
        _registerServiceMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CashRegisterDto?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessCashCutAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_IfGrandTotalZero()
    {
        var dto = new CreateCashCutDto 
        { 
            // All zeros
            TotalThousands = 0,
            TotalSlips = 0,
            CashRegisterName = "Reg",
            SupervisorName = "Sup",
            CashierName = "Cash",
            BranchName = "Branch"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
    }
}
