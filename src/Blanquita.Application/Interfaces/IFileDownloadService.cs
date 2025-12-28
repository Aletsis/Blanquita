namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para manejar la descarga de archivos en el cliente.
/// Abstrae la lógica de interoperabilidad con JavaScript.
/// </summary>
public interface IFileDownloadService
{
    /// <summary>
    /// Descarga un archivo en el navegador del cliente.
    /// </summary>
    /// <param name="contenido">Contenido del archivo en bytes</param>
    /// <param name="nombreArchivo">Nombre del archivo a descargar</param>
    /// <param name="contentType">Tipo MIME del archivo (ej: application/pdf, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DescargarArchivoAsync(
        byte[] contenido, 
        string nombreArchivo, 
        string contentType,
        CancellationToken cancellationToken = default);
}
