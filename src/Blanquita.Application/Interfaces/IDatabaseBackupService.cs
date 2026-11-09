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
    /// Restaura la base de datos a partir de un archivo de respaldo del directorio de respaldos.
    /// El respaldo se restaura primero en una base temporal; solo si tiene éxito se reemplaza la base actual
    /// (las conexiones activas se cierran en ese momento) y se aplican las migraciones pendientes.
    /// Si la restauración falla, la base de datos actual no se modifica.
    /// </summary>
    /// <param name="backupFilePath">Ruta completa del archivo de respaldo (.backup).</param>
    Task RestoreBackupAsync(string backupFilePath);

    /// <summary>
    /// Guarda en el directorio de respaldos un archivo subido por el usuario. El nombre se limpia y,
    /// si ya existe un archivo con ese nombre, se agrega un sufijo para no sobrescribirlo.
    /// </summary>
    /// <returns>Ruta completa del archivo guardado.</returns>
    Task<string> SaveUploadedBackupAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la lista de respaldos disponibles en el servidor.
    /// </summary>
    /// <param name="directoryPath">Directorio opcional a consultar. Si es nulo, usa el predeterminado.</param>
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? directoryPath = null);

    /// <summary>
    /// Elimina un archivo de respaldo específico del directorio de respaldos.
    /// </summary>
    /// <param name="backupFilePath">Ruta completa del archivo de respaldo.</param>
    Task DeleteBackupAsync(string backupFilePath);

    /// <summary>
    /// Obtiene la ruta del directorio configurado para guardar los respaldos.
    /// </summary>
    Task<string> GetBackupDirectoryAsync();
}
