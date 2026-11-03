using System.Diagnostics;
using System.IO;
using System.Text;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Servicio de infraestructura para realizar respaldos y restauración de bases de datos PostgreSQL.
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService
{
    private const string BackupExtension = ".backup";

    // PostgreSQL limita los identificadores a 63 bytes
    private const int MaxIdentifierLength = 63;

    private static readonly HashSet<char> InvalidFileNameChars = new() { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

    // Respaldos y restauraciones no deben ejecutarse en paralelo (p. ej. el respaldo programado de las 2:00
    // durante una restauración manual). La aplicación corre en una sola instancia.
    private static readonly SemaphoreSlim OperationLock = new(1, 1);

    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseBackupService> _logger;

    public DatabaseBackupService(IConfiguration configuration, ILogger<DatabaseBackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> CreateBackupAsync(string? destinationDirectory = null)
    {
        var connection = GetConnectionInfo();
        var pgDumpPath = FindPgToolPath("pg_dump");

        var backupDir = string.IsNullOrEmpty(destinationDirectory) ? GetDefaultBackupDirectory() : destinationDirectory;
        Directory.CreateDirectory(backupDir);

        var fileName = $"blanquita_backup_{DateTime.Now:yyyyMMdd_HHmmss}{BackupExtension}";
        var backupFilePath = Path.Combine(backupDir, fileName);

        await OperationLock.WaitAsync();
        try
        {
            _logger.LogInformation("Iniciando respaldo de base de datos {Database} en {Path}", connection.Database, backupFilePath);

            var (exitCode, errorOutput) = await RunPgToolAsync(pgDumpPath, connection.Password,
                "-h", connection.Host, "-p", connection.Port.ToString(), "-U", connection.Username,
                "-F", "c", "-b", "-v", "-f", backupFilePath, connection.Database);

            if (exitCode != 0)
            {
                // No dejar un archivo incompleto que luego parezca un respaldo válido
                TryDeleteFile(backupFilePath);
                _logger.LogError("Error al ejecutar pg_dump. Código de salida: {ExitCode}. Error: {Error}", exitCode, errorOutput);
                throw new InvalidOperationException($"Error al crear el respaldo de la base de datos: {errorOutput}");
            }
        }
        finally
        {
            OperationLock.Release();
        }

        _logger.LogInformation("Respaldo creado exitosamente: {Path}", backupFilePath);

        // Purgar respaldos antiguos según la configuración
        await PruneOldBackupsAsync(backupDir);

        return backupFilePath;
    }

    /// <inheritdoc />
    public async Task RestoreBackupAsync(string backupFilePath)
    {
        var fullPath = EnsureInsideBackupDirectory(backupFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"El archivo de respaldo no existe: {fullPath}");
        }

        var connection = GetConnectionInfo();
        var pgRestorePath = FindPgToolPath("pg_restore");

        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var tempDatabase = BuildDatabaseName(connection.Database, $"_restore_{stamp}");
        var previousDatabase = BuildDatabaseName(connection.Database, $"_prev_{stamp}");

        await OperationLock.WaitAsync();
        try
        {
            await using var adminConnection = await OpenMaintenanceConnectionAsync(connection);

            // 1. Restaurar en una base temporal: si falla, la base actual queda intacta
            _logger.LogInformation("Restaurando {Path} en la base temporal {TempDatabase}", fullPath, tempDatabase);
            await ExecuteAsync(adminConnection,
                $"CREATE DATABASE {QuoteIdentifier(tempDatabase)} WITH OWNER = {QuoteIdentifier(connection.Username)} ENCODING = 'UTF8';");

            var (exitCode, errorOutput) = await RunPgToolAsync(pgRestorePath, connection.Password,
                "-h", connection.Host, "-p", connection.Port.ToString(), "-U", connection.Username,
                "-d", tempDatabase, "-v", fullPath);

            // Código de salida 0 es éxito. Código 1 indica advertencias menores (normalmente aceptable).
            // Código > 1 indica errores críticos.
            if (exitCode > 1)
            {
                _logger.LogError("Error al ejecutar pg_restore. Código de salida: {ExitCode}. Error: {Error}", exitCode, errorOutput);
                await TryDropDatabaseAsync(adminConnection, tempDatabase);
                throw new InvalidOperationException($"Error al restaurar la base de datos (la base actual no se modificó): {errorOutput}");
            }

            // 2. Intercambiar la base actual por la restaurada
            await SwapDatabasesAsync(adminConnection, connection.Database, tempDatabase, previousDatabase);

            // Las conexiones del pool apuntaban a la base anterior
            NpgsqlConnection.ClearAllPools();

            // 3. Eliminar la base anterior (ya no tiene conexiones porque se deshabilitaron)
            await TryDropDatabaseAsync(adminConnection, previousDatabase);
        }
        finally
        {
            OperationLock.Release();
        }

        // 4. El respaldo puede ser de una versión anterior del esquema: aplicar migraciones pendientes
        try
        {
            await ApplyMigrationsAsync(connection.ConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "La base de datos se restauró, pero no se pudieron aplicar las migraciones pendientes");
            throw new InvalidOperationException(
                "La base de datos se restauró, pero no se pudieron aplicar las migraciones. Reinicie la aplicación para reintentarlo.", ex);
        }

        _logger.LogInformation("Restauración de base de datos completada con éxito desde {Path}.", fullPath);
    }

    /// <inheritdoc />
    public async Task<string> SaveUploadedBackupAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        var safeName = SanitizeFileName(originalFileName);
        if (!safeName.EndsWith(BackupExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El archivo debe ser un respaldo con extensión .backup");
        }

        var backupDir = GetDefaultBackupDirectory();
        Directory.CreateDirectory(backupDir);

        var baseName = Path.GetFileNameWithoutExtension(safeName);
        var destinationPath = Path.Combine(backupDir, safeName);
        for (var i = 1; File.Exists(destinationPath); i++)
        {
            destinationPath = Path.Combine(backupDir, $"{baseName}_{i}{BackupExtension}");
        }

        try
        {
            // CreateNew evita sobrescribir si otro proceso creó el archivo entre la verificación y la escritura
            await using var fileStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write);
            await content.CopyToAsync(fileStream, cancellationToken);
        }
        catch
        {
            TryDeleteFile(destinationPath);
            throw;
        }

        _logger.LogInformation("Respaldo subido y guardado en {Path}", destinationPath);
        return destinationPath;
    }

    /// <inheritdoc />
    public Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? directoryPath = null)
    {
        var backupDir = string.IsNullOrEmpty(directoryPath) ? GetDefaultBackupDirectory() : directoryPath;

        if (!Directory.Exists(backupDir))
        {
            return Task.FromResult(new List<BackupFileInfo>());
        }

        var files = Directory.GetFiles(backupDir, "*" + BackupExtension)
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new BackupFileInfo
                {
                    FileName = info.Name,
                    FilePath = info.FullName,
                    SizeBytes = info.Length,
                    CreatedAt = info.CreationTime
                };
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public Task DeleteBackupAsync(string backupFilePath)
    {
        var fullPath = EnsureInsideBackupDirectory(backupFilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Archivo de respaldo eliminado: {Path}", fullPath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetBackupDirectoryAsync()
    {
        return Task.FromResult(GetDefaultBackupDirectory());
    }

    private string GetDefaultBackupDirectory()
    {
        var backupDir = _configuration["DatabaseBackup:BackupDirectory"];
        if (string.IsNullOrEmpty(backupDir))
        {
            backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        }
        return Path.GetFullPath(backupDir);
    }

    /// <summary>
    /// Solo se permite operar sobre archivos .backup dentro del directorio de respaldos.
    /// </summary>
    private string EnsureInsideBackupDirectory(string backupFilePath)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            throw new ArgumentException("La ruta del respaldo es obligatoria.", nameof(backupFilePath));
        }

        var backupDir = GetDefaultBackupDirectory();
        var fullPath = Path.GetFullPath(backupFilePath);
        var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;

        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(Path.TrimEndingDirectorySeparator(directory), Path.TrimEndingDirectorySeparator(backupDir), comparison) ||
            !fullPath.EndsWith(BackupExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Solo se permite operar sobre archivos .backup del directorio de respaldos.");
        }

        return fullPath;
    }

    internal static string SanitizeFileName(string originalFileName)
    {
        // Quitar cualquier ruta enviada por el navegador y caracteres no válidos
        var name = Path.GetFileName((originalFileName ?? string.Empty).Replace('\\', '/'));
        // Conjunto fijo (caracteres no válidos en Windows) para que el resultado no dependa del sistema operativo
        var cleaned = new string(name.Select(c => InvalidFileNameChars.Contains(c) || char.IsControl(c) ? '_' : c).ToArray())
            .TrimStart(' ')
            .TrimEnd(' ', '.');

        if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(cleaned)))
        {
            cleaned = $"respaldo_subido_{DateTime.Now:yyyyMMdd_HHmmss}{BackupExtension}";
        }

        return cleaned;
    }

    private (string Host, int Port, string Database, string Username, string Password, string ConnectionString) GetConnectionInfo()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'DefaultConnection'.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return (
            builder.Host ?? "localhost",
            builder.Port,
            builder.Database ?? "BlanquitaDB",
            builder.Username ?? "postgres",
            builder.Password ?? "",
            connectionString);
    }

    private static async Task<NpgsqlConnection> OpenMaintenanceConnectionAsync(
        (string Host, int Port, string Database, string Username, string Password, string ConnectionString) connection)
    {
        var builder = new NpgsqlConnectionStringBuilder(connection.ConnectionString)
        {
            Database = "postgres",
            // Las conexiones de mantenimiento no deben quedar en el pool de la aplicación
            Pooling = false
        };

        var adminConnection = new NpgsqlConnection(builder.ConnectionString);
        await adminConnection.OpenAsync();
        return adminConnection;
    }

    private async Task SwapDatabasesAsync(NpgsqlConnection adminConnection, string targetDatabase, string restoredDatabase, string previousDatabase)
    {
        var target = QuoteIdentifier(targetDatabase);

        // Bloquear conexiones nuevas (la aplicación y Hangfire reconectan de inmediato) y cerrar las actuales
        await ExecuteAsync(adminConnection, $"ALTER DATABASE {target} WITH ALLOW_CONNECTIONS false;");
        try
        {
            await using (var terminate = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @database AND pid <> pg_backend_pid();",
                adminConnection))
            {
                terminate.Parameters.AddWithValue("database", targetDatabase);
                await terminate.ExecuteNonQueryAsync();
            }

            await ExecuteAsync(adminConnection, $"ALTER DATABASE {target} RENAME TO {QuoteIdentifier(previousDatabase)};");
        }
        catch
        {
            // No se renombró: volver a permitir conexiones a la base actual
            await TryExecuteAsync(adminConnection, $"ALTER DATABASE {target} WITH ALLOW_CONNECTIONS true;");
            await TryDropDatabaseAsync(adminConnection, restoredDatabase);
            throw;
        }

        try
        {
            await ExecuteAsync(adminConnection, $"ALTER DATABASE {QuoteIdentifier(restoredDatabase)} RENAME TO {target};");
        }
        catch
        {
            // Revertir: devolver su nombre a la base original y permitir conexiones de nuevo
            _logger.LogError("No se pudo activar la base restaurada; se revierte a la base original");
            await TryExecuteAsync(adminConnection, $"ALTER DATABASE {QuoteIdentifier(previousDatabase)} RENAME TO {target};");
            await TryExecuteAsync(adminConnection, $"ALTER DATABASE {target} WITH ALLOW_CONNECTIONS true;");
            await TryDropDatabaseAsync(adminConnection, restoredDatabase);
            throw;
        }

        _logger.LogInformation("Base de datos {Database} reemplazada por la versión restaurada", targetDatabase);
    }

    private static async Task ApplyMigrationsAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(BlanquitaDbContext).Assembly.FullName))
            .Options;

        await using var context = new BlanquitaDbContext(options);
        await context.Database.MigrateAsync();
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task TryExecuteAsync(NpgsqlConnection connection, string sql)
    {
        try
        {
            await ExecuteAsync(connection, sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar el comando de mantenimiento: {Sql}", sql);
        }
    }

    private async Task TryDropDatabaseAsync(NpgsqlConnection connection, string database)
    {
        try
        {
            await ExecuteAsync(connection, $"DROP DATABASE IF EXISTS {QuoteIdentifier(database)};");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar la base de datos {Database}; puede eliminarse manualmente", database);
        }
    }

    internal static string QuoteIdentifier(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";

    internal static string BuildDatabaseName(string baseName, string suffix)
    {
        var maxBaseLength = MaxIdentifierLength - suffix.Length;
        var trimmedBase = baseName.Length > maxBaseLength ? baseName[..maxBaseLength] : baseName;
        return trimmedBase + suffix;
    }

    /// <summary>
    /// Ejecuta pg_dump/pg_restore con argumentos separados (sin concatenar una línea de comandos)
    /// y devuelve el código de salida y la salida de error.
    /// </summary>
    private async Task<(int ExitCode, string ErrorOutput)> RunPgToolAsync(string toolPath, string password, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            // La salida estándar no se usa (el resultado va a archivo/BD); no redirigirla evita
            // que el proceso se bloquee si llenara el búfer sin que nadie lo lea
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.EnvironmentVariables["PGPASSWORD"] = password;

        using var process = new Process { StartInfo = startInfo };

        var errorOutput = new StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                lock (errorOutput)
                {
                    errorOutput.AppendLine(e.Data);
                }
                _logger.LogDebug("[{Tool}]: {Data}", Path.GetFileNameWithoutExtension(toolPath), e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        lock (errorOutput)
        {
            return (process.ExitCode, errorOutput.ToString());
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el archivo {Path}", path);
        }
    }

    private string FindPgToolPath(string baseToolName)
    {
        var isWindows = OperatingSystem.IsWindows();
        var toolName = isWindows ? $"{baseToolName}.exe" : baseToolName;

        // 1. Verificar si está en la configuración
        var configPath = _configuration["DatabaseBackup:PostgresBinPath"];
        if (!string.IsNullOrEmpty(configPath))
        {
            var fullPath = Path.Combine(configPath, toolName);
            if (File.Exists(fullPath)) return fullPath;
        }

        // 2. Verificar rutas de instalación estándar
        if (isWindows)
        {
            var standardVersions = new[] { "17", "16", "15", "14", "13", "12" };
            foreach (var version in standardVersions)
            {
                var standardPath = Path.Combine(@"C:\Program Files\PostgreSQL", version, "bin", toolName);
                if (File.Exists(standardPath)) return standardPath;
            }
        }
        else
        {
            var standardPaths = new[] { "/usr/bin", "/usr/local/bin", "/usr/local/pgsql/bin" };
            foreach (var path in standardPaths)
            {
                var fullPath = Path.Combine(path, toolName);
                if (File.Exists(fullPath)) return fullPath;
            }
        }

        // 3. Buscar en el PATH del sistema
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path.Trim(), toolName);
                if (File.Exists(fullPath)) return fullPath;
            }
        }

        throw new FileNotFoundException($"No se pudo encontrar la herramienta '{toolName}' de PostgreSQL. " +
            "Por favor, instale las herramientas de PostgreSQL o configure la ruta correcta en 'DatabaseBackup:PostgresBinPath' en el archivo appsettings.json.");
    }

    private async Task PruneOldBackupsAsync(string backupDir)
    {
        try
        {
            if (!int.TryParse(_configuration["DatabaseBackup:KeepLastNBackups"], out int keepCount))
            {
                keepCount = 10; // Mantener 10 respaldos por defecto
            }

            var backups = await GetAvailableBackupsAsync(backupDir);
            if (backups.Count > keepCount)
            {
                var toDelete = backups.OrderByDescending(b => b.CreatedAt).Skip(keepCount).ToList();
                foreach (var backup in toDelete)
                {
                    _logger.LogInformation("Eliminando respaldo antiguo por límite de almacenamiento: {Path}", backup.FilePath);
                    if (File.Exists(backup.FilePath))
                    {
                        File.Delete(backup.FilePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al purgar los respaldos antiguos.");
        }
    }
}
