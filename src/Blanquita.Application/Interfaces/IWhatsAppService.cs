using System.Threading.Tasks;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Interfaz para interactuar con el microservicio de sincronización de WhatsApp.
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Envía un mensaje de texto por WhatsApp a un número determinado.
    /// </summary>
    /// <param name="number">Número de teléfono destino (10 dígitos o con prefijo)</param>
    /// <param name="message">Cuerpo del mensaje</param>
    /// <returns>True si el mensaje se envió exitosamente, de lo contrario False.</returns>
    Task<bool> SendMessageAsync(string number, string message);

    /// <summary>
    /// Envía un archivo/documento adjunto por WhatsApp a un número determinado.
    /// </summary>
    /// <param name="number">Número de teléfono destino</param>
    /// <param name="fileBase64">Contenido del archivo codificado en Base64</param>
    /// <param name="fileName">Nombre del archivo con extensión</param>
    /// <param name="mimeType">Tipo MIME del archivo</param>
    /// <param name="caption">Texto opcional que acompaña al documento</param>
    /// <returns>True si se envió exitosamente, de lo contrario False.</returns>
    Task<bool> SendDocumentAsync(string number, string fileBase64, string fileName, string mimeType, string? caption = null);
}
