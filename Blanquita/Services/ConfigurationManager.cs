using Blanquita.Models;
using Blanquita.Interfaces;

namespace Blanquita.Services
{
    public interface IConfigurationManager
    {
        AppConfiguration CargarConfiguracion();
        void GuardarConfiguracion(AppConfiguration config);
    }

    public class ConfigurationManager : IConfigurationManager
    {
        private readonly string _configPath;
        private readonly string _filePath;

        public ConfigurationManager()
        {
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "BillingReportSystem"
            );
            _filePath = Path.Combine(_configPath, "config.json");
        }

        public AppConfiguration CargarConfiguracion()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return System.Text.Json.JsonSerializer.Deserialize<AppConfiguration>(json)
                       ?? new AppConfiguration();
            }
            return new AppConfiguration();
        }

        public void GuardarConfiguracion(AppConfiguration config)
        {
            if (!Directory.Exists(_configPath))
                Directory.CreateDirectory(_configPath);

            var json = System.Text.Json.JsonSerializer.Serialize(config,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_filePath, json);
        }
    }
}
