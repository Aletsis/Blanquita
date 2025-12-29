using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Servicio de diagnóstico para archivos FoxPro/DBF.
/// </summary>
public interface IFoxProDiagnosticService
{
    /// <summary>
    /// Diagnostica un archivo DBF y retorna información detallada.
    /// </summary>
    /// <param name="path">Ruta del archivo a diagnosticar</param>
    /// <param name="expectedColumns">Columnas esperadas (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del diagnóstico</returns>
    Task<DiagnosticoResultado> DiagnoseFileAsync(
        string path, 
        List<string>? expectedColumns = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros de muestra de un archivo DBF.
    /// </summary>
    /// <param name="path">Ruta del archivo</param>
    /// <param name="count">Cantidad de registros a obtener</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de registros como diccionarios</returns>
    Task<List<Dictionary<string, object>>> GetSampleRecordsAsync(
        string path, 
        int count, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica la conexión con los archivos FoxPro.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la conexión es exitosa</returns>
    Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default);
}
