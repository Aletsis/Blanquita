using Blanquita.Models;
using System.Text.Json;

namespace Blanquita.Services
{
    public class FileConfigService
    {
        private readonly string _configPath;
        private DbfSettings _config;

        private readonly ILogger<FileConfigService> _logger;

        public FileConfigService(IWebHostEnvironment env, ILogger<FileConfigService> logger)
        {
            _configPath = Path.Combine(env.ContentRootPath, "Config/appsettings.custom.json");
            _logger = logger;
            LoadConfig();
        }

        public DbfSettings Config => _config;

        public void UpdateConfig(Action<DbfSettings> updateAction)
        {
            updateAction(_config);
            SaveConfig();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<DbfSettings>(json) ?? new DbfSettings();
                }
                else
                {
                    _config = new DbfSettings();
                    SaveConfig(); // Crea el archivo si no existe
                }
            }
            catch
            {
                _config = new DbfSettings(); // Configuración por defecto si hay error
            }
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando configuración");
            }
        }
    }
}
