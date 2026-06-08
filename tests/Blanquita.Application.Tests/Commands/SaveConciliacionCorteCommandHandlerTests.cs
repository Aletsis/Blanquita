using Blanquita.Application.Commands.Conciliaciones;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Commands;

public class SaveConciliacionCorteCommandHandlerTests
{
    private readonly Mock<IConciliacionService> _mockConciliacionService;
    private readonly Mock<ILogger<SaveConciliacionCorteCommandHandler>> _mockLogger;
    private readonly SaveConciliacionCorteCommandHandler _handler;

    public SaveConciliacionCorteCommandHandlerTests()
    {
        _mockConciliacionService = new Mock<IConciliacionService>();
        _mockLogger = new Mock<ILogger<SaveConciliacionCorteCommandHandler>>();
        _handler = new SaveConciliacionCorteCommandHandler(_mockConciliacionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidData_CalculatesCorrectlyAndSaves()
    {
        // Arrange
        var command = new SaveConciliacionCorteCommand(
            ShiftId: 123,
            BranchName: "Sucursal Chapultepec",
            CashRegisterName: "Caja 1",
            CashierName: "Maria Garcia",
            TotalRecolecciones: 1500m,
            EfectivoEntregado: 500m,
            Banregio: 300m,
            Banbajio: 200m,
            ReturnsTotal: 100m,
            TotalSold: 2500m
        );

        _mockConciliacionService
            .Setup(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        // Verify the mathematical calculation was correct
        // totalEfectivo = 1500 + 500 = 2000
        // totalTarjetas = 300 + 200 = 500
        // totalEntregado = 2000 + 500 = 2500
        // totalEsperado = 2500 - 100 = 2400
        // diferencia = 2500 - 2400 = 100
        _mockConciliacionService.Verify(s => s.SaveConciliacionCorteAsync(It.Is<ConciliacionCorteDto>(dto =>
            dto.AperturaId == 123 &&
            dto.Sucursal == "Sucursal Chapultepec" &&
            dto.Caja == "Caja 1" &&
            dto.Cajero == "Maria Garcia" &&
            dto.TotalRecolecciones == 1500m &&
            dto.EfectivoEntregado == 500m &&
            dto.TotalEfectivo == 2000m &&
            dto.Banregio == 300m &&
            dto.Banbajio == 200m &&
            dto.TotalTarjetas == 500m &&
            dto.Devoluciones == 100m &&
            dto.TotalEntregado == 2500m &&
            dto.TotalEsperado == 2400m &&
            dto.Diferencia == 100m
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
