using Blanquita.Application.Interfaces;
using Microsoft.JSInterop;

namespace Blanquita.Web.Services;

/// <summary>
/// Implementación del servicio de descarga de archivos usando JavaScript Interop.
/// </summary>
public class FileDownloadService : IFileDownloadService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<FileDownloadService> _logger;

    public FileDownloadService(IJSRuntime js, ILogger<FileDownloadService> logger)
    {
        _js = js ?? throw new ArgumentNullException(nameof(js));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DescargarArchivoAsync(
        byte[] contenido, 
        string nombreArchivo, 
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (contenido == null || contenido.Length == 0)
            throw new ArgumentException("El contenido del archivo no puede estar vacío", nameof(contenido));

        if (string.IsNullOrWhiteSpace(nombreArchivo))
            throw new ArgumentException("El nombre del archivo no puede estar vacío", nameof(nombreArchivo));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("El tipo de contenido no puede estar vacío", nameof(contentType));

        try
        {
            _logger.LogInformation(
                "Iniciando descarga de archivo: {FileName} ({Size} bytes, tipo: {ContentType})", 
                nombreArchivo, 
                contenido.Length, 
                contentType);

            var base64 = Convert.ToBase64String(contenido);
            await _js.InvokeVoidAsync(
                "fileDownloadHelper.downloadFile", 
                cancellationToken,
                nombreArchivo, 
                contentType, 
                base64);

            _logger.LogInformation("Descarga completada exitosamente: {FileName}", nombreArchivo);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Descarga cancelada por el usuario: {FileName}", nombreArchivo);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo: {FileName}", nombreArchivo);
            throw;
        }
    }
}
