using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Interfaz para gestionar las copias de seguridad (respaldo) y restauración de la base de datos.
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Crea un respaldo completo de la base de datos en formato binario comprimido (.backup).
    /// </summary>
    /// <param name="destinationDirectory">Directorio opcional de destino. Si es nulo, usa el predeterminado.</param>
    /// <returns>Ruta completa del archivo de respaldo creado.</returns>
    Task<string> CreateBackupAsync(string? destinationDirectory = null);

    /// <summary>
    /// Restaura la base de datos a partir de un archivo de respaldo.
    /// ¡ADVERTENCIA! Esta operación terminará todas las conexiones activas, eliminará la base de datos actual y la volverá a crear antes de restaurar.
    /// </summary>
    /// <param name="backupFilePath">Ruta completa del archivo de respaldo (.backup).</param>
    Task RestoreBackupAsync(string backupFilePath);

    /// <summary>
    /// Obtiene la lista de respaldos disponibles en el servidor.
    /// </summary>
    /// <param name="directoryPath">Directorio opcional a consultar. Si es nulo, usa el predeterminado.</param>
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? directoryPath = null);

    /// <summary>
    /// Elimina un archivo de respaldo específico.
    /// </summary>
    /// <param name="backupFilePath">Ruta completa del archivo de respaldo.</param>
    Task DeleteBackupAsync(string backupFilePath);

    /// <summary>
    /// Obtiene la ruta del directorio configurado para guardar los respaldos.
    /// </summary>
    Task<string> GetBackupDirectoryAsync();
}
