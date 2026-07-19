namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa la información de un archivo de respaldo de base de datos en el servidor.
/// </summary>
public class BackupFileInfo
{
    /// <summary>
    /// Nombre del archivo de respaldo (ej: blanquita_backup_20260629_120000.backup)
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Ruta completa en el sistema de archivos del servidor
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Fecha de creación del archivo de respaldo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
