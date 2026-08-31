using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Servicio para interactuar con el microservicio de WhatsApp a través de HTTP.
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguracionService configuracionService,
        ILogger<WhatsAppService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuracionService = configuracionService ?? throw new ArgumentNullException(nameof(configuracionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> SendMessageAsync(string number, string message)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            _logger.LogWarning("Intento de enviar mensaje de WhatsApp con número vacío.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Intento de enviar mensaje de WhatsApp con cuerpo vacío.");
            return false;
        }

        try
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            
            if (!config.IsWhatsAppEnabled)
            {
                _logger.LogInformation("Envío de WhatsApp omitido: El servicio está deshabilitado en la configuración.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.WhatsAppServiceUrl))
            {
                _logger.LogWarning("No se puede enviar mensaje por WhatsApp: La URL del servicio no está configurada.");
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            
            // Si la API Key está configurada, inyectar en cabecera
            if (!string.IsNullOrWhiteSpace(config.WhatsAppApiKey))
            {
                client.DefaultRequestHeaders.Remove("X-API-Key");
                client.DefaultRequestHeaders.Add("X-API-Key", config.WhatsAppApiKey);
            }

            var requestUri = $"{config.WhatsAppServiceUrl.TrimEnd('/')}/send";
            var payload = new
            {
                number = number,
                message = message
            };

            _logger.LogInformation("Enviando petición HTTP POST a {RequestUri} para {Number}", requestUri, number);
            
            var response = await client.PostAsJsonAsync(requestUri, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Mensaje de WhatsApp enviado exitosamente a {Number}", number);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("El servicio de WhatsApp devolvió un error. Status: {StatusCode}, Respuesta: {Response}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invocar el microservicio de WhatsApp para el número {Number}", number);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendDocumentAsync(string number, string fileBase64, string fileName, string mimeType, string? caption = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            _logger.LogWarning("Intento de enviar documento de WhatsApp con número vacío.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(fileBase64))
        {
            _logger.LogWarning("Intento de enviar documento de WhatsApp con contenido Base64 vacío.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            _logger.LogWarning("Intento de enviar documento de WhatsApp con nombre de archivo vacío.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            _logger.LogWarning("Intento de enviar documento de WhatsApp con tipo MIME vacío.");
            return false;
        }

        try
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            
            if (!config.IsWhatsAppEnabled)
            {
                _logger.LogInformation("Envío de documento por WhatsApp omitido: El servicio está deshabilitado en la configuración.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.WhatsAppServiceUrl))
            {
                _logger.LogWarning("No se puede enviar documento por WhatsApp: La URL del servicio no está configurada.");
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            
            // Si la API Key está configurada, inyectar en cabecera
            if (!string.IsNullOrWhiteSpace(config.WhatsAppApiKey))
            {
                client.DefaultRequestHeaders.Remove("X-API-Key");
                client.DefaultRequestHeaders.Add("X-API-Key", config.WhatsAppApiKey);
            }

            var requestUri = $"{config.WhatsAppServiceUrl.TrimEnd('/')}/send-document";
            var payload = new
            {
                number = number,
                fileBase64 = fileBase64,
                fileName = fileName,
                mimeType = mimeType,
                caption = caption
            };

            _logger.LogInformation("Enviando petición HTTP POST de documento a {RequestUri} para {Number}", requestUri, number);
            
            var response = await client.PostAsJsonAsync(requestUri, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Documento de WhatsApp enviado exitosamente a {Number}", number);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("El servicio de WhatsApp devolvió un error al enviar documento. Status: {StatusCode}, Respuesta: {Response}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invocar el microservicio de WhatsApp para enviar documento al número {Number}", number);
            return false;
        }
    }
}
