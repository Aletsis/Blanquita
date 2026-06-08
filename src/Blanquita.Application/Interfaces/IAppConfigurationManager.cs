using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Interfaz para gestionar la configuración de la aplicación (Capa de Aplicación)
/// </summary>
public interface IAppConfigurationManager
{
    /// <summary>
    /// Carga la configuración desde el archivo
    /// </summary>
    LegacyAppConfiguration CargarConfiguracion();

    /// <summary>
    /// Guarda la configuración en el archivo
    /// </summary>
    void GuardarConfiguracion(LegacyAppConfiguration config);

    /// <summary>
    /// Valida que una ruta de archivo existe
    /// </summary>
    bool ValidatePath(string path);
}
