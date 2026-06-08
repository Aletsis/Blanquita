using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Queries.Cashiers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blanquita.Application.Tests.Queries;

public class GetCashierByContpaqIdQueryHandlerTests
{
    private readonly Mock<ICashierService> _mockCashierService;
    private readonly Mock<ILogger<GetCashierByContpaqIdQueryHandler>> _mockLogger;
    private readonly GetCashierByContpaqIdQueryHandler _handler;

    public GetCashierByContpaqIdQueryHandlerTests()
    {
        _mockCashierService = new Mock<ICashierService>();
        _mockLogger = new Mock<ILogger<GetCashierByContpaqIdQueryHandler>>();
        _handler = new GetCashierByContpaqIdQueryHandler(_mockCashierService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsMatchingCashier()
    {
        // Arrange
        var branchId = 1;
        var contpaqId = 55;
        var cashiers = new List<CashierDto>
        {
            new CashierDto { Id = 10, Name = "Cajera A", BranchId = branchId, IDContpaq = 55 },
            new CashierDto { Id = 20, Name = "Cajera B", BranchId = branchId, IDContpaq = 99 }
        };

        _mockCashierService
            .Setup(s => s.GetByBranchAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashiers);

        var query = new GetCashierByContpaqIdQuery(branchId, contpaqId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cajera A", result.Name);
        Assert.Equal(55, result.IDContpaq);
        _mockCashierService.Verify(s => s.GetByBranchAsync(branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonMatchingId_ReturnsNull()
    {
        // Arrange
        var branchId = 1;
        var contpaqId = 999;
        var cashiers = new List<CashierDto>
        {
            new CashierDto { Id = 10, Name = "Cajera A", BranchId = branchId, IDContpaq = 55 }
        };

        _mockCashierService
            .Setup(s => s.GetByBranchAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashiers);

        var query = new GetCashierByContpaqIdQuery(branchId, contpaqId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
