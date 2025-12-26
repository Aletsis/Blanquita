using Blanquita.Models;

namespace Blanquita.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportarExcelAsync(Reporte reporte);
        Task<byte[]> ExportarPDFAsync(Reporte reporte);
        Task DescargarArchivoAsync(byte[] contenido, string nombreArchivo, string mimeType);
    }
}