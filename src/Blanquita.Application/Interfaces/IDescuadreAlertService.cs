using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Envía las alertas de descuadre financiero a los correos configurados y a los supervisores de la sucursal.
/// </summary>
public interface IDescuadreAlertService
{
    /// <summary>
    /// Envía la alerta. Los errores de cada destinatario se registran y no detienen a los demás,
    /// así el trabajo no falla ni Hangfire lo reintenta (lo que duplicaría los mensajes ya enviados).
    /// </summary>
    Task SendAsync(DescuadreAlertDto alert, CancellationToken cancellationToken = default);
}
