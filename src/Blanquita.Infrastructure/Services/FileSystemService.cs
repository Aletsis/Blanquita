using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de sistema de archivos
/// Proporciona acceso controlado al sistema de archivos del servidor
/// </summary>
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<string>> GetAvailableDrivesAsync()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.Name)
                .ToList();

            _logger.LogDebug("Found {Count} available drives", drives.Count);
            return Task.FromResult<IEnumerable<string>>(drives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available drives");
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    public Task<IEnumerable<string>> GetDirectoriesAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
            }

            var directories = Directory.GetDirectories(path)
                .OrderBy(d => d)
                .ToList();

            _logger.LogDebug("Found {Count} directories in {Path}", directories.Count, path);
            return Task.FromResult<IEnumerable<string>>(directories);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to directory: {Path}", path);
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting directories from {Path}", path);
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    public Task<IEnumerable<string>> GetDbfFilesAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
            }

            var files = Directory.GetFiles(path, "*.dbf", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToList();

            _logger.LogDebug("Found {Count} DBF files in {Path}", files.Count, path);
            return Task.FromResult<IEnumerable<string>>(files);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to directory: {Path}", path);
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DBF files from {Path}", path);
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    public bool FileExists(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var exists = File.Exists(filePath);
            _logger.LogDebug("File exists check for {FilePath}: {Exists}", filePath, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists: {FilePath}", filePath);
            return false;
        }
    }

    public bool ValidateFileName(string filePath, string expectedFileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(expectedFileName))
            {
                return false;
            }

            var actualFileName = Path.GetFileName(filePath);
            var isValid = actualFileName.Equals(expectedFileName, StringComparison.OrdinalIgnoreCase);
            
            _logger.LogDebug(
                "File name validation for {FilePath}: Expected={Expected}, Actual={Actual}, Valid={Valid}",
                filePath, expectedFileName, actualFileName, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file name for {FilePath}", filePath);
            return false;
        }
    }

    public string? GetParentDirectory(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var parent = Directory.GetParent(path);
            var parentPath = parent?.FullName;
            
            _logger.LogDebug("Parent directory of {Path}: {Parent}", path, parentPath ?? "null");
            return parentPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent directory for {Path}", path);
            return null;
        }
    }

    public string GetFileName(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            var fileName = Path.GetFileName(filePath);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file name from {FilePath}", filePath);
            return string.Empty;
        }
    }

    public bool HasDirectoryAccess(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return false;
            }

            // Intenta listar el contenido para verificar permisos
            _ = Directory.GetFileSystemEntries(path);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("No access to directory: {Path}", path);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking directory access for {Path}", path);
            return false;
        }
    }
}
