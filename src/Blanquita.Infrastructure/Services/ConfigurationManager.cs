using Blanquita.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Interfaz para gestionar la configuración de la aplicación
/// </summary>
public interface IAppConfigurationManager
{
    /// <summary>
    /// Carga la configuración desde el archivo
    /// </summary>
    AppConfiguration CargarConfiguracion();

    /// <summary>
    /// Guarda la configuración en el archivo
    /// </summary>
    void GuardarConfiguracion(AppConfiguration config);

    /// <summary>
    /// Valida que una ruta de archivo existe
    /// </summary>
    bool ValidatePath(string path);
}

/// <summary>
/// Implementación del gestor de configuración
/// </summary>
public class AppConfigurationManager : IAppConfigurationManager
{
    private readonly string _configPath;
    private readonly string _filePath;
    private readonly ILogger<AppConfigurationManager> _logger;

    public AppConfigurationManager(ILogger<AppConfigurationManager> logger)
    {
        _logger = logger;
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "BillingReportSystem"
        );
        _filePath = Path.Combine(_configPath, "config.json");
    }

    /// <inheritdoc/>
    public AppConfiguration CargarConfiguracion()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                _logger.LogInformation("Cargando configuración desde {FilePath}", _filePath);
                var json = File.ReadAllText(_filePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
                _logger.LogInformation("Configuración cargada exitosamente");
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la configuración desde {FilePath}", _filePath);
                return new AppConfiguration();
            }
        }

        _logger.LogWarning("Archivo de configuración no encontrado en {FilePath}. Usando configuración predeterminada", _filePath);
        return new AppConfiguration();
    }

    /// <inheritdoc/>
    public void GuardarConfiguracion(AppConfiguration config)
    {
        try
        {
            if (!Directory.Exists(_configPath))
            {
                _logger.LogInformation("Creando directorio de configuración en {ConfigPath}", _configPath);
                Directory.CreateDirectory(_configPath);
            }

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
            _logger.LogInformation("Configuración guardada exitosamente en {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar la configuración en {FilePath}", _filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool ValidatePath(string path)
    {
        var exists = !string.IsNullOrEmpty(path) && File.Exists(path);
        
        if (!exists && !string.IsNullOrEmpty(path))
        {
            _logger.LogWarning("Ruta no válida o archivo no encontrado: {Path}", path);
        }
        
        return exists;
    }
}
