using System.Diagnostics;
using System.IO;
using System.Text;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Servicio de infraestructura para realizar respaldos y restauración de bases de datos PostgreSQL.
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService
{
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
        var connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'DefaultConnection'.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = builder.Host ?? "localhost";
        var port = builder.Port;
        var database = builder.Database ?? "BlanquitaDB";
        var username = builder.Username ?? "postgres";
        var password = builder.Password ?? "";

        var pgDumpPath = FindPgToolPath("pg_dump.exe");

        var backupDir = destinationDirectory;
        if (string.IsNullOrEmpty(backupDir))
        {
            backupDir = _configuration["DatabaseBackup:BackupDirectory"];
            if (string.IsNullOrEmpty(backupDir))
            {
                backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
            }
        }

        if (!Directory.Exists(backupDir))
        {
            Directory.CreateDirectory(backupDir);
        }

        var fileName = $"blanquita_backup_{DateTime.Now:yyyyMMdd_HHmmss}.backup";
        var backupFilePath = Path.Combine(backupDir, fileName);

        _logger.LogInformation("Iniciando respaldo de base de datos {Database} en {Path}", database, backupFilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = pgDumpPath,
            Arguments = $"-h {host} -p {port} -U {username} -F c -b -v -f \"{backupFilePath}\" \"{database}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.EnvironmentVariables["PGPASSWORD"] = password;

        using var process = new Process { StartInfo = startInfo };
        
        var errorOutput = new StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorOutput.AppendLine(e.Data);
                _logger.LogDebug("[pg_dump]: {Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("Error al ejecutar pg_dump. Código de salida: {ExitCode}. Error: {Error}", process.ExitCode, errorOutput.ToString());
            throw new InvalidOperationException($"Error al crear el respaldo de la base de datos: {errorOutput}");
        }

        _logger.LogInformation("Respaldo creado exitosamente: {Path}", backupFilePath);

        // Purgar respaldos antiguos según la configuración
        await PruneOldBackupsAsync(backupDir);

        return backupFilePath;
    }

    /// <inheritdoc />
    public async Task RestoreBackupAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException($"El archivo de respaldo no existe: {backupFilePath}");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'DefaultConnection'.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = builder.Host ?? "localhost";
        var port = builder.Port;
        var database = builder.Database ?? "BlanquitaDB";
        var username = builder.Username ?? "postgres";
        var password = builder.Password ?? "";

        var pgRestorePath = FindPgToolPath("pg_restore.exe");

        _logger.LogInformation("Preparando la base de datos para la restauración de {Path}", backupFilePath);

        // Terminar conexiones, eliminar y volver a crear la base de datos
        await PrepareDatabaseForRestoreAsync(host, port, username, password, database);

        _logger.LogInformation("Iniciando restauración de base de datos {Database} desde {Path}", database, backupFilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = pgRestorePath,
            Arguments = $"-h {host} -p {port} -U {username} -d \"{database}\" -v \"{backupFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.EnvironmentVariables["PGPASSWORD"] = password;

        using var process = new Process { StartInfo = startInfo };
        
        var errorOutput = new StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorOutput.AppendLine(e.Data);
                _logger.LogDebug("[pg_restore]: {Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        // Código de salida 0 es éxito. Código 1 indica advertencias menores (normalmente aceptable).
        // Código > 1 indica errores críticos.
        if (process.ExitCode > 1)
        {
            _logger.LogError("Error al ejecutar pg_restore. Código de salida: {ExitCode}. Error: {Error}", process.ExitCode, errorOutput.ToString());
            throw new InvalidOperationException($"Error al restaurar la base de datos: {errorOutput}");
        }

        _logger.LogInformation("Restauración de base de datos completada con éxito.");
    }

    /// <inheritdoc />
    public Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? directoryPath = null)
    {
        var backupDir = directoryPath;
        if (string.IsNullOrEmpty(backupDir))
        {
            backupDir = _configuration["DatabaseBackup:BackupDirectory"];
            if (string.IsNullOrEmpty(backupDir))
            {
                backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
            }
        }

        if (!Directory.Exists(backupDir))
        {
            return Task.FromResult(new List<BackupFileInfo>());
        }

        var files = Directory.GetFiles(backupDir, "*.backup")
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
        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
            _logger.LogInformation("Archivo de respaldo eliminado: {Path}", backupFilePath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetBackupDirectoryAsync()
    {
        var backupDir = _configuration["DatabaseBackup:BackupDirectory"];
        if (string.IsNullOrEmpty(backupDir))
        {
            backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        }
        return Task.FromResult(backupDir);
    }

    private string FindPgToolPath(string toolName)
    {
        // 1. Verificar si está en la configuración
        var configPath = _configuration["DatabaseBackup:PostgresBinPath"];
        if (!string.IsNullOrEmpty(configPath))
        {
            var fullPath = Path.Combine(configPath, toolName);
            if (File.Exists(fullPath)) return fullPath;
        }

        // 2. Verificar rutas de instalación estándar en Windows
        var standardVersions = new[] { "17", "16", "15", "14", "13", "12" };
        foreach (var version in standardVersions)
        {
            var standardPath = Path.Combine(@"C:\Program Files\PostgreSQL", version, "bin", toolName);
            if (File.Exists(standardPath)) return standardPath;
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

    private async Task PrepareDatabaseForRestoreAsync(string host, int port, string username, string password, string targetDatabase)
    {
        var postgresConnString = $"Host={host};Port={port};Database=postgres;Username={username};Password={password};TrustServerCertificate=True;";
        using var conn = new NpgsqlConnection(postgresConnString);
        await conn.OpenAsync();

        // 1. Terminar todas las conexiones activas a la base de datos objetivo
        var terminateQuery = $@"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{targetDatabase}'
              AND pid <> pg_backend_pid();";
        
        using (var cmd = new NpgsqlCommand(terminateQuery, conn))
        {
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al intentar terminar conexiones activas de la base de datos {Database}", targetDatabase);
            }
        }

        // 2. Eliminar la base de datos si existe
        var dropQuery = $"DROP DATABASE IF EXISTS \"{targetDatabase}\";";
        using (var cmd = new NpgsqlCommand(dropQuery, conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // 3. Volver a crear la base de datos vacía
        var createQuery = $"CREATE DATABASE \"{targetDatabase}\" WITH OWNER = \"{username}\" ENCODING = 'UTF8';";
        using (var cmd = new NpgsqlCommand(createQuery, conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
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
