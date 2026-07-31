using Blanquita.Application.Commands.Conciliaciones;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Reflection;

namespace Blanquita.Application.Tests.Commands;

public class SaveConciliacionCorteCommandHandlerTests
{
    private readonly Mock<IConciliacionService> _mockConciliacionService;
    private readonly Mock<IFoxProShiftRepository> _mockShiftRepository;
    private readonly Mock<ILogger<SaveConciliacionCorteCommandHandler>> _mockLogger;
    private readonly Mock<IConfiguracionService> _mockConfiguracionService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IWhatsAppService> _mockWhatsAppService;
    private readonly Mock<ISupervisorRepository> _mockSupervisorRepository;
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly SaveConciliacionCorteCommandHandler _handler;

    public SaveConciliacionCorteCommandHandlerTests()
    {
        _mockConciliacionService = new Mock<IConciliacionService>();
        _mockShiftRepository = new Mock<IFoxProShiftRepository>();
        _mockLogger = new Mock<ILogger<SaveConciliacionCorteCommandHandler>>();
        _mockConfiguracionService = new Mock<IConfiguracionService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockWhatsAppService = new Mock<IWhatsAppService>();
        _mockSupervisorRepository = new Mock<ISupervisorRepository>();
        _mockBranchRepository = new Mock<IBranchRepository>();

        _handler = new SaveConciliacionCorteCommandHandler(
            _mockConciliacionService.Object,
            _mockShiftRepository.Object,
            _mockLogger.Object,
            _mockConfiguracionService.Object,
            _mockEmailService.Object,
            _mockWhatsAppService.Object,
            _mockSupervisorRepository.Object,
            _mockBranchRepository.Object);
    }

    private static T SetId<T>(T entity, int id) where T : BaseEntity
    {
        var property = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        property?.SetValue(entity, id);
        return entity;
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
            TotalSold: 2600m,
            Fecha: DateTime.UtcNow
        );

        _mockShiftRepository
            .Setup(s => s.GetShiftDataAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftConciliationDataDto
            {
                InternalId = 123,
                IdContpaqi = 1,
                CashierId = 1,
                OpeningTime = DateTime.Now.AddHours(-8),
                ClosingTime = DateTime.Now,
                Status = 1, // Closed
                CashCollected = 2000m,
                CardCollected = 500m,
                ReturnsTotal = 100m
            });

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
        // totalEsperado = 2600 - 100 = 2500
        // diferencia = 2500 - 2500 = 0m
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
            dto.TotalEsperado == 2500m &&
            dto.Diferencia == 0m &&
            dto.Fecha == command.Fecha
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OpenShift_ThrowsInvalidOperationException()
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
            TotalSold: 2500m,
            Fecha: DateTime.UtcNow
        );

        _mockShiftRepository
            .Setup(s => s.GetShiftDataAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftConciliationDataDto
            {
                InternalId = 123,
                IdContpaqi = 1,
                CashierId = 1,
                OpeningTime = DateTime.Now.AddHours(-8),
                ClosingTime = null,
                Status = 0, // Open
                CashCollected = 2000m,
                CardCollected = 500m,
                ReturnsTotal = 100m
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("No se puede guardar la conciliación de corte para el turno 123 porque no ha sido cerrado en CONTPAQi POS.", exception.Message);

        _mockConciliacionService.Verify(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DiscrepancyAtOrOverThreshold_SendsEmailAndWhatsAppAlerts()
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
            TotalSold: 2700m,
            Fecha: DateTime.UtcNow
        );

        _mockShiftRepository
            .Setup(s => s.GetShiftDataAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftConciliationDataDto
            {
                InternalId = 123,
                IdContpaqi = 1,
                CashierId = 1,
                OpeningTime = DateTime.Now.AddHours(-8),
                ClosingTime = DateTime.Now,
                Status = 1, // Closed
                CashCollected = 2000m,
                CardCollected = 500m,
                ReturnsTotal = 100m
            });

        _mockConciliacionService
            .Setup(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var branch = Branch.Create("Sucursal Chapultepec", "CH", "S", "G", "D", "Dir", "C");
        SetId(branch, 42);

        _mockBranchRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { branch });

        _mockConfiguracionService
            .Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { AlertEmails = "supervisor@test.com" });

        var supervisor1 = Supervisor.Create(101, "John Doe", 42, "3312345678", true);
        var supervisor2 = Supervisor.Create(102, "Jane Doe", 42, "3387654321", true);
        var supervisorInactive = Supervisor.Create(103, "Inactive Supervisor", 42, "3399999999", false);
        var supervisorNoPhone = Supervisor.Create(104, "No Phone Supervisor", 42, null, true);

        _mockSupervisorRepository
            .Setup(r => r.GetByBranchAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supervisor> { supervisor1, supervisor2, supervisorInactive, supervisorNoPhone });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _mockConciliacionService.Verify(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Once);

        _mockEmailService.Verify(e => e.SendEmailAsync(
            "supervisor@test.com", 
            It.Is<string>(s => s.Contains("Alerta de Descuadre")), 
            It.Is<string>(b => b.Contains("Maria Garcia")), 
            It.IsAny<IEnumerable<string>>()), 
            Times.Once);

        _mockWhatsAppService.Verify(w => w.SendMessageAsync(
            "3312345678", 
            It.Is<string>(m => m.Contains("ALERTA DE DESCUADRE"))), 
            Times.Once);

        _mockWhatsAppService.Verify(w => w.SendMessageAsync(
            "3387654321", 
            It.Is<string>(m => m.Contains("ALERTA DE DESCUADRE"))), 
            Times.Once);

        _mockWhatsAppService.Verify(w => w.SendMessageAsync("3399999999", It.IsAny<string>()), Times.Never);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync(string.Empty, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlertSendingFailure_StillSavesConciliationSuccessfully()
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
            TotalSold: 2700m,
            Fecha: DateTime.UtcNow
        );

        _mockShiftRepository
            .Setup(s => s.GetShiftDataAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftConciliationDataDto
            {
                InternalId = 123,
                IdContpaqi = 1,
                CashierId = 1,
                OpeningTime = DateTime.Now.AddHours(-8),
                ClosingTime = DateTime.Now,
                Status = 1, // Closed
                CashCollected = 2000m,
                CardCollected = 500m,
                ReturnsTotal = 100m
            });

        _mockConciliacionService
            .Setup(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockBranchRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("DB Connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _mockConciliacionService.Verify(s => s.SaveConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
