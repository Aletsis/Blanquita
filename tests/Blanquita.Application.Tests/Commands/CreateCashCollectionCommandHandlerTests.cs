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

public class CreateCashCollectionCommandHandlerTests
{
    private readonly Mock<ICashCollectionService> _mockCashCollectionService;
    private readonly Mock<ICashRegisterService> _mockCashRegisterService;
    private readonly Mock<IPrintingService> _mockPrintingService;
    private readonly Mock<ILogger<CreateCashCollectionCommandHandler>> _mockLogger;
    private readonly CreateCashCollectionCommandHandler _handler;

    public CreateCashCollectionCommandHandlerTests()
    {
        _mockCashCollectionService = new Mock<ICashCollectionService>();
        _mockCashRegisterService = new Mock<ICashRegisterService>();
        _mockPrintingService = new Mock<IPrintingService>();
        _mockLogger = new Mock<ILogger<CreateCashCollectionCommandHandler>>();

        _handler = new CreateCashCollectionCommandHandler(
            _mockCashCollectionService.Object,
            _mockCashRegisterService.Object,
            _mockPrintingService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ValidData_CreatesCollectionAndPrintsSuccessfully()
    {
        // Arrange
        var dto = new CreateCashCollectionDto
        {
            Thousands = 1,
            FiveHundreds = 2,
            CashRegisterName = "Caja Principal",
            SupervisorName = "Supervisor A",
            CashierName = "Cajera A",
            IsForCashCut = false
        };

        var collectionDto = new CashCollectionDto
        {
            Id = 1,
            Folio = 101,
            TotalAmount = 2000,
            CashRegisterName = "Caja Principal",
            SupervisorName = "Supervisor A",
            CashierName = "Cajera A"
        };

        var registerDto = new CashRegisterDto
        {
            Id = 10,
            Name = "Caja Principal",
            PrinterIp = "192.168.1.50",
            PrinterPort = 9100
        };

        _mockCashCollectionService
            .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionDto);

        _mockCashRegisterService
            .Setup(s => s.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerDto);

        _mockPrintingService
            .Setup(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCashCollectionCommand(dto, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.PrintingSucceeded);
        Assert.False(result.UsedBackupPrinter);
        Assert.Equal(collectionDto, result.CashCollection);

        _mockCashCollectionService.Verify(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        _mockCashRegisterService.Verify(s => s.GetByIdAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _mockPrintingService.Verify(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PrimaryPrinterFails_PrintsToBackupSuccessfully()
    {
        // Arrange
        var dto = new CreateCashCollectionDto
        {
            Thousands = 1,
            FiveHundreds = 2,
            CashRegisterName = "Caja Principal",
            SupervisorName = "Supervisor A",
            CashierName = "Cajera A",
            IsForCashCut = false
        };

        var collectionDto = new CashCollectionDto
        {
            Id = 1,
            Folio = 101,
            TotalAmount = 2000,
            CashRegisterName = "Caja Principal",
            SupervisorName = "Supervisor A",
            CashierName = "Cajera A"
        };

        var registerDto = new CashRegisterDto
        {
            Id = 10,
            Name = "Caja Principal",
            PrinterIp = "192.168.1.50",
            PrinterPort = 9100
        };

        var backupRegisterDto = new CashRegisterDto
        {
            Id = 20,
            Name = "Caja Respaldo",
            PrinterIp = "192.168.1.60",
            PrinterPort = 9100
        };

        _mockCashCollectionService
            .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionDto);

        _mockCashRegisterService
            .Setup(s => s.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerDto);

        _mockPrintingService
            .Setup(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network printer offline"));

        _mockCashRegisterService
            .Setup(s => s.GetBackupRegisterAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(backupRegisterDto);

        _mockPrintingService
            .Setup(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.60", 9100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCashCollectionCommand(dto, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.PrintingSucceeded);
        Assert.True(result.UsedBackupPrinter);
        Assert.Contains("Caja Respaldo", result.Message);

        _mockCashCollectionService.Verify(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        _mockCashRegisterService.Verify(s => s.GetByIdAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _mockPrintingService.Verify(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.50", 9100, It.IsAny<CancellationToken>()), Times.Once);
        _mockCashRegisterService.Verify(s => s.GetBackupRegisterAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _mockPrintingService.Verify(s => s.PrintCashCollectionAsync(collectionDto, "192.168.1.60", 9100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
