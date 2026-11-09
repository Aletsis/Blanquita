using System.Globalization;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

/// <summary>
/// Envía las alertas de descuadre por correo y WhatsApp. Se ejecuta desde la cola de Hangfire.
/// </summary>
public class DescuadreAlertService : IDescuadreAlertService
{
    /// <summary>Diferencia (en valor absoluto) a partir de la cual se envía la alerta.</summary>
    public const decimal Threshold = 50.00m;

    // El trabajo corre fuera de la petición del usuario: la moneda se formatea siempre en pesos mexicanos
    private static readonly CultureInfo Mx = CultureInfo.GetCultureInfo("es-MX");

    private readonly IConfiguracionService _configuracionService;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ISupervisorRepository _supervisorRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<DescuadreAlertService> _logger;

    public DescuadreAlertService(
        IConfiguracionService configuracionService,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ISupervisorRepository supervisorRepository,
        IBranchRepository branchRepository,
        ILogger<DescuadreAlertService> logger)
    {
        _configuracionService = configuracionService;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _supervisorRepository = supervisorRepository;
        _branchRepository = branchRepository;
        _logger = logger;
    }

    public async Task SendAsync(DescuadreAlertDto alert, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enviando alertas de descuadre de {Diferencia} para el turno {ShiftId}", alert.Diferencia, alert.ShiftId);

        await SendEmailAlertsAsync(alert);
        await SendWhatsAppAlertsAsync(alert, cancellationToken);
    }

    private async Task SendEmailAlertsAsync(DescuadreAlertDto alert)
    {
        try
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            if (config == null || string.IsNullOrWhiteSpace(config.AlertEmails))
            {
                return;
            }

            var emails = config.AlertEmails
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e));

            var subject = $"⚠️ Alerta de Descuadre Financiero - Sucursal: {alert.BranchName}";
            var body = $@"
<h3>Alerta de Descuadre Financiero Detectado</h3>
<p>Se ha registrado un descuadre en el corte del turno <strong>{alert.ShiftId}</strong> que supera el límite de {Threshold.ToString("C", Mx)} MXN.</p>
<table>
    <tr><td><strong>Sucursal:</strong></td><td>{System.Net.WebUtility.HtmlEncode(alert.BranchName)}</td></tr>
    <tr><td><strong>Caja:</strong></td><td>{System.Net.WebUtility.HtmlEncode(alert.CashRegisterName)}</td></tr>
    <tr><td><strong>Cajero:</strong></td><td>{System.Net.WebUtility.HtmlEncode(alert.CashierName)}</td></tr>
    <tr><td><strong>Total Entregado:</strong></td><td>{alert.TotalEntregado.ToString("C", Mx)}</td></tr>
    <tr><td><strong>Total Esperado:</strong></td><td>{alert.TotalEsperado.ToString("C", Mx)}</td></tr>
    <tr><td><strong>Diferencia:</strong></td><td><strong style='color:red;'>{alert.Diferencia.ToString("C", Mx)}</strong></td></tr>
</table>
<br/>
<p>Por favor revise la conciliación correspondiente.</p>";

            foreach (var email in emails)
            {
                try
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                    _logger.LogInformation("Alerta de descuadre enviada a {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de alerta a {Email}", email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el envío de alertas por correo.");
        }
    }

    private async Task SendWhatsAppAlertsAsync(DescuadreAlertDto alert, CancellationToken cancellationToken)
    {
        try
        {
            var branches = await _branchRepository.GetAllAsync(cancellationToken);
            var branch = branches.FirstOrDefault(b => string.Equals(b.Name, alert.BranchName, StringComparison.OrdinalIgnoreCase));
            if (branch == null)
            {
                _logger.LogWarning("No se encontró una sucursal con el nombre '{BranchName}' para enviar alertas de descuadre por WhatsApp.", alert.BranchName);
                return;
            }

            var supervisors = await _supervisorRepository.GetByBranchAsync(branch.Id, cancellationToken);
            var activeSupervisorsWithPhone = supervisors
                .Where(s => s.IsActive && !string.IsNullOrWhiteSpace(s.PhoneNumber));

            var waMessage = $"⚠️ *ALERTA DE DESCUADRE* ⚠️\n\n" +
                            $"Se registró un descuadre de *{alert.Diferencia.ToString("C", Mx)}* en el corte del turno *{alert.ShiftId}*.\n\n" +
                            $"*Detalles:*\n" +
                            $"• Sucursal: {alert.BranchName}\n" +
                            $"• Caja: {alert.CashRegisterName}\n" +
                            $"• Cajero: {alert.CashierName}\n" +
                            $"• Total Entregado: {alert.TotalEntregado.ToString("C", Mx)}\n" +
                            $"• Total Esperado: {alert.TotalEsperado.ToString("C", Mx)}";

            foreach (var supervisor in activeSupervisorsWithPhone)
            {
                try
                {
                    await _whatsAppService.SendMessageAsync(supervisor.PhoneNumber!, waMessage);
                    _logger.LogInformation("Alerta de descuadre por WhatsApp enviada al supervisor {SupervisorName} ({Phone})", supervisor.Name, supervisor.PhoneNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar alerta por WhatsApp al supervisor {SupervisorName} ({Phone})", supervisor.Name, supervisor.PhoneNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el envío de alertas por WhatsApp.");
        }
    }
}
