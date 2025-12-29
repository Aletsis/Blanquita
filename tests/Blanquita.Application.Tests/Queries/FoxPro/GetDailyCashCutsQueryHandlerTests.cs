using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetDailyCashCuts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetDailyCashCutsQueryHandlerTests
{
    private readonly Mock<IFoxProCashCutRepository> _mockRepository;
    private readonly Mock<ILogger<GetDailyCashCutsQueryHandler>> _mockLogger;
    private readonly GetDailyCashCutsQueryHandler _handler;

    public GetDailyCashCutsQueryHandlerTests()
    {
        _mockRepository = new Mock<IFoxProCashCutRepository>();
        _mockLogger = new Mock<ILogger<GetDailyCashCutsQueryHandler>>();
        _handler = new GetDailyCashCutsQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CashCutsExist_ReturnsCashCuts()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;
        var expectedCashCuts = new List<CashCutDto>
        {
            new CashCutDto
            {
                CutDateTime = date,
                CashRegisterName = "Caja 1",
                RawInvoices = "1000",
                RawReturns = "50",
                CashRegisterId = 1
            },
            new CashCutDto
            {
                CutDateTime = date,
                CashRegisterName = "Caja 2",
                RawInvoices = "2000",
                RawReturns = "100",
                CashRegisterId = 2
            }
        };

        _mockRepository
            .Setup(r => r.GetDailyCashCutsAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCashCuts);

        var query = new GetDailyCashCutsQuery(date, branchId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedCashCuts[0].CashRegisterName, resultList[0].CashRegisterName);
        Assert.Equal(expectedCashCuts[1].RawInvoices, resultList[1].RawInvoices);

        _mockRepository.Verify(r => r.GetDailyCashCutsAsync(date, branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoCashCutsFound_ReturnsEmptyList()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;

        _mockRepository
            .Setup(r => r.GetDailyCashCutsAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashCutDto>());

        var query = new GetDailyCashCutsQuery(date, branchId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetDailyCashCutsAsync(date, branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FutureDate_StillCallsRepository()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(10);
        var branchId = 1;

        _mockRepository
            .Setup(r => r.GetDailyCashCutsAsync(futureDate, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashCutDto>());

        var query = new GetDailyCashCutsQuery(futureDate, branchId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetDailyCashCutsAsync(futureDate, branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository
            .Setup(r => r.GetDailyCashCutsAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new GetDailyCashCutsQuery(date, branchId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }
}
