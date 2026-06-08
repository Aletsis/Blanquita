using Blanquita.Application.Commands.Cajas;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blanquita.Application.Tests.Commands;

public class ProcessCashCutCommandHandlerTests
{
    private readonly Mock<ICashCutService> _mockCashCutService;
    private readonly Mock<ICashRegisterService> _mockCashRegisterService;
    private readonly Mock<IPrintingService> _mockPrintingService;
    private readonly Mock<ILogger<ProcessCashCutCommandHandler>> _mockLogger;
    private readonly ProcessCashCutCommandHandler _handler;

    public ProcessCashCutCommandHandlerTests()
    {
        _mockCashCutService = new Mock<ICashCutService>();
        _mockCashRegisterService = new Mock<ICashRegisterService>();
        _mockPrintingService = new Mock<IPrintingService>();
        _mockLogger = new Mock<ILogger<ProcessCashCutCommandHandler>>();

        _handler = new ProcessCashCutCommandHandler(
            _mockCashCutService.Object,
            _mockCashRegisterService.Object,
            _mockPrintingService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ValidData_ProcessesCutAndPrintsSuccessfully()
    {
        // Arrange
        var request = new ProcessCashCutRequest
        {
            SupervisorId = 1,
            CashierId = 2,
            CashRegisterId = 3,
            TotalSlips = 5000,
            TotalBanbajio = 1000,
            TotalBanregio = 2000
        };

        var cashCutDto = new CashCutDto
        {
            Id = 100,
            CashRegisterName = "Caja Principal",
            SupervisorName = "Supervisor A",
            CashierName = "Cajera A",
            CutDateTime = DateTime.Now
        };

        var cashRegisterDto = new CashRegisterDto
        {
            Id = 3,
            Name = "Caja Principal",
            PrinterIp = "192.168.1.50",
            PrinterPort = 9100
        };

        _mockCashCutService
            .Setup(s => s.ProcessCashCutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashCutDto);

        _mockCashRegisterService
            .Setup(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashRegisterDto);

        _mockPrintingService
            .Setup(s => s.PrintCashCutAsync(cashCutDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ProcessCashCutCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.PrintingSucceeded);
        Assert.Equal(cashCutDto, result.CashCut);

        _mockCashCutService.Verify(s => s.ProcessCashCutAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockCashRegisterService.Verify(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>()), Times.Once);
        _mockPrintingService.Verify(s => s.PrintCashCutAsync(cashCutDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
