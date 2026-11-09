using System.Reflection;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Services;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class DescuadreAlertServiceTests
{
    private readonly Mock<IConfiguracionService> _mockConfiguracionService = new();
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<IWhatsAppService> _mockWhatsAppService = new();
    private readonly Mock<ISupervisorRepository> _mockSupervisorRepository = new();
    private readonly Mock<IBranchRepository> _mockBranchRepository = new();
    private readonly DescuadreAlertService _service;

    private static readonly DescuadreAlertDto Alert = new()
    {
        ShiftId = 123,
        BranchName = "Sucursal Chapultepec",
        CashRegisterName = "Caja 1",
        CashierName = "Maria <b>Garcia</b>",
        TotalEntregado = 2500m,
        TotalEsperado = 2600m,
        Diferencia = -100m
    };

    public DescuadreAlertServiceTests()
    {
        _service = new DescuadreAlertService(
            _mockConfiguracionService.Object,
            _mockEmailService.Object,
            _mockWhatsAppService.Object,
            _mockSupervisorRepository.Object,
            _mockBranchRepository.Object,
            new Mock<ILogger<DescuadreAlertService>>().Object);
    }

    private static T SetId<T>(T entity, int id) where T : BaseEntity
    {
        typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(entity, id);
        return entity;
    }

    private void SetupBranchWithSupervisors()
    {
        var branch = SetId(Branch.Create("Sucursal Chapultepec", "CH", "S", "G", "D", "Dir", "C"), 42);
        _mockBranchRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Branch> { branch });

        _mockSupervisorRepository
            .Setup(r => r.GetByBranchAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supervisor>
            {
                Supervisor.Create(101, "John Doe", 42, "3312345678", true),
                Supervisor.Create(102, "Jane Doe", 42, "3387654321", true),
                Supervisor.Create(103, "Inactive Supervisor", 42, "3399999999", false),
                Supervisor.Create(104, "No Phone Supervisor", 42, null, true)
            });
    }

    [Fact]
    public async Task SendAsync_SendsEmailsAndWhatsAppToActiveSupervisorsWithPhone()
    {
        SetupBranchWithSupervisors();
        _mockConfiguracionService
            .Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { AlertEmails = "a@test.com; b@test.com" });

        await _service.SendAsync(Alert);

        _mockEmailService.Verify(e => e.SendEmailAsync(
            "a@test.com",
            It.Is<string>(s => s.Contains("Alerta de Descuadre")),
            It.Is<string>(b => b.Contains("Maria &lt;b&gt;Garcia&lt;/b&gt;") && b.Contains("-$100.00")),
            It.IsAny<IEnumerable<string>>()), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync("b@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Once);

        _mockWhatsAppService.Verify(w => w.SendMessageAsync("3312345678", It.Is<string>(m => m.Contains("ALERTA DE DESCUADRE"))), Times.Once);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync("3387654321", It.IsAny<string>()), Times.Once);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync("3399999999", It.IsAny<string>()), Times.Never);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAsync_FailuresAreIsolated_AndDoNotThrow()
    {
        // Si el trabajo lanzara, Hangfire lo reintentaría y duplicaría los mensajes ya enviados
        SetupBranchWithSupervisors();
        _mockConfiguracionService
            .Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { AlertEmails = "a@test.com" });
        _mockEmailService
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new Exception("SMTP caído"));
        _mockWhatsAppService
            .Setup(w => w.SendMessageAsync("3312345678", It.IsAny<string>()))
            .ThrowsAsync(new Exception("WhatsApp caído"));

        await _service.SendAsync(Alert);

        // El fallo del primer supervisor no impide avisar al segundo
        _mockWhatsAppService.Verify(w => w.SendMessageAsync("3387654321", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_BranchLookupFails_StillSendsEmails()
    {
        _mockBranchRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("BD caída"));
        _mockConfiguracionService
            .Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { AlertEmails = "a@test.com" });

        await _service.SendAsync(Alert);

        _mockEmailService.Verify(e => e.SendEmailAsync("a@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Once);
        _mockWhatsAppService.Verify(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
