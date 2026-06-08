using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Queries.Conciliaciones;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blanquita.Application.Tests.Queries;

public class GetConciliacionesReportQueryHandlerTests
{
    private readonly Mock<IConciliacionService> _mockConciliacionService;
    private readonly Mock<ICashRegisterService> _mockCashRegisterService;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<ILogger<GetConciliacionesReportQueryHandler>> _mockLogger;
    private readonly GetConciliacionesReportQueryHandler _handler;

    public GetConciliacionesReportQueryHandlerTests()
    {
        _mockConciliacionService = new Mock<IConciliacionService>();
        _mockCashRegisterService = new Mock<ICashRegisterService>();
        _mockBranchService = new Mock<IBranchService>();
        _mockLogger = new Mock<ILogger<GetConciliacionesReportQueryHandler>>();
        
        _handler = new GetConciliacionesReportQueryHandler(
            _mockConciliacionService.Object,
            _mockCashRegisterService.Object,
            _mockBranchService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ValidData_RetrievesCorrectReportAndConvertsCorrectly()
    {
        // Arrange
        var branchName = "Sucursal Chapultepec";
        var date = DateTime.Today;

        var branches = new List<BranchDto>
        {
            new BranchDto { Id = 1, Name = branchName }
        };

        var conciliaciones = new List<ConciliacionCorteDto>
        {
            new ConciliacionCorteDto { Caja = "Caja B", Diferencia = 0 },
            new ConciliacionCorteDto { Caja = "Caja A", Diferencia = -10m }
        };

        var availableBoxes = new List<AvailableBoxDto>
        {
            new AvailableBoxDto { Id = 1, Name = "Caja A" }
        };

        var cashRegisters = new List<CashRegisterDto>
        {
            new CashRegisterDto { Id = 10, Name = "Caja A" },
            new CashRegisterDto { Id = 20, Name = "Caja B" }
        };

        _mockBranchService
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        _mockConciliacionService
            .Setup(s => s.GetConciliacionesByBranchAndDateAsync(branchName, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conciliaciones);

        _mockConciliacionService
            .Setup(s => s.GetAvailableBoxesAsync(date, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableBoxes);

        _mockCashRegisterService
            .Setup(s => s.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashRegisters);

        var query = new GetConciliacionesReportQuery(branchName, date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PendingBoxesCount); // 1 available box found
        
        // Assert the sorting by CashRegister ID in PostgreSQL:
        // Caja A has ID 10
        // Caja B has ID 20
        // Therefore, Caja A should come before Caja B in the result
        var sortedList = result.Conciliaciones.ToList();
        Assert.Equal(2, sortedList.Count);
        Assert.Equal("Caja A", sortedList[0].Caja);
        Assert.Equal("Caja B", sortedList[1].Caja);

        _mockConciliacionService.Verify(s => s.GetConciliacionesByBranchAndDateAsync(branchName, date, It.IsAny<CancellationToken>()), Times.Once);
        _mockConciliacionService.Verify(s => s.GetAvailableBoxesAsync(date, 1, It.IsAny<CancellationToken>()), Times.Once);
        _mockCashRegisterService.Verify(s => s.GetByBranchAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
