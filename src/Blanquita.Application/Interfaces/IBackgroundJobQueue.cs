using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Encola trabajos para ejecutarse en segundo plano, fuera de la petición del usuario.
/// </summary>
public interface IBackgroundJobQueue
{
    /// <summary>Encola el envío por correo y WhatsApp de una alerta de descuadre.</summary>
    void EnqueueDescuadreAlert(DescuadreAlertDto alert);
}
