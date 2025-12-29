namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para operaciones del sistema de archivos
/// Abstrae el acceso al sistema de archivos para mantener Clean Architecture
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Obtiene las unidades de disco disponibles en el sistema
    /// </summary>
    /// <returns>Lista de nombres de unidades (ej: "C:\", "D:\")</returns>
    Task<IEnumerable<string>> GetAvailableDrivesAsync();

    /// <summary>
    /// Obtiene los directorios dentro de una ruta específica
    /// </summary>
    /// <param name="path">Ruta del directorio padre</param>
    /// <returns>Lista de rutas completas de los subdirectorios</returns>
    Task<IEnumerable<string>> GetDirectoriesAsync(string path);

    /// <summary>
    /// Obtiene los archivos DBF dentro de una ruta específica
    /// </summary>
    /// <param name="path">Ruta del directorio</param>
    /// <returns>Lista de rutas completas de archivos .DBF</returns>
    Task<IEnumerable<string>> GetDbfFilesAsync(string path);

    /// <summary>
    /// Verifica si un archivo existe
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo</param>
    /// <returns>True si el archivo existe, False en caso contrario</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Valida que un archivo tenga el nombre esperado
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo</param>
    /// <param name="expectedFileName">Nombre esperado del archivo</param>
    /// <returns>True si el nombre coincide (case-insensitive), False en caso contrario</returns>
    bool ValidateFileName(string filePath, string expectedFileName);

    /// <summary>
    /// Obtiene el directorio padre de una ruta
    /// </summary>
    /// <param name="path">Ruta actual</param>
    /// <returns>Ruta del directorio padre, o null si no existe</returns>
    string? GetParentDirectory(string path);

    /// <summary>
    /// Obtiene solo el nombre del archivo de una ruta completa
    /// </summary>
    /// <param name="filePath">Ruta completa del archivo</param>
    /// <returns>Nombre del archivo sin la ruta</returns>
    string GetFileName(string filePath);

    /// <summary>
    /// Verifica si el usuario tiene permisos para acceder a un directorio
    /// </summary>
    /// <param name="path">Ruta del directorio</param>
    /// <returns>True si tiene acceso, False en caso contrario</returns>
    bool HasDirectoryAccess(string path);
}
