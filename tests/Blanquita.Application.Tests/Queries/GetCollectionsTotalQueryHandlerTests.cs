using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Queries.Cajas;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blanquita.Application.Tests.Queries;

public class GetCollectionsTotalQueryHandlerTests
{
    private readonly Mock<ICashCollectionService> _mockCashCollectionService;
    private readonly Mock<ILogger<GetCollectionsTotalQueryHandler>> _mockLogger;
    private readonly GetCollectionsTotalQueryHandler _handler;

    public GetCollectionsTotalQueryHandlerTests()
    {
        _mockCashCollectionService = new Mock<ICashCollectionService>();
        _mockLogger = new Mock<ILogger<GetCollectionsTotalQueryHandler>>();
        _handler = new GetCollectionsTotalQueryHandler(_mockCashCollectionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CalculatesDenominationsTotalCorrectly()
    {
        // Arrange
        var registerName = "Caja Principal";
        var date = DateTime.Today;

        var collections = new List<CashCollectionDto>
        {
            new CashCollectionDto
            {
                Thousands = 1,     // 1000
                FiveHundreds = 2,  // 1000
                TwoHundreds = 3,   // 600
                Hundreds = 4,      // 400
                Fifties = 5,       // 250
                Twenties = 6       // 120
                                   // Total = 3370
            }
        };

        _mockCashCollectionService
            .Setup(s => s.SearchAsync(It.IsAny<SearchCashCollectionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(collections);

        var query = new GetCollectionsTotalQuery(registerName, date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3370m, result);
        _mockCashCollectionService.Verify(
            s => s.SearchAsync(
                It.Is<SearchCashCollectionRequest>(r => 
                    r.CashRegisterName == registerName && 
                    r.IsCut == false && 
                    r.FechaInicio == date.Date
                ), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once
        );
    }
}
