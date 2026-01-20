using Blanquita.Application.DTOs;
using Blanquita.Domain.Enums;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para gestionar la configuración del sistema
/// </summary>
public interface IConfiguracionService
{
    /// <summary>
    /// Obtiene la configuración actual del sistema
    /// </summary>
    Task<ConfiguracionDto> ObtenerConfiguracionAsync();

    /// <summary>
    /// Guarda la configuración del sistema
    /// </summary>
    /// <param name="configuracion">Configuración a guardar</param>
    Task GuardarConfiguracionAsync(ConfiguracionDto configuracion);

    /// <summary>
    /// Valida la configuración del sistema
    /// </summary>
    /// <param name="configuracion">Configuración a validar</param>
    Task<ResultadoValidacionConfiguracion> ValidarConfiguracionAsync(ConfiguracionDto configuracion);

    /// <summary>
    /// Valida que una ruta de archivo DBF existe
    /// </summary>
    /// <param name="ruta">Ruta del archivo</param>
    bool ValidarRutaArchivo(string ruta);

    /// <summary>
    /// Valida que una ruta de directorio existe
    /// </summary>
    /// <param name="ruta">Ruta del directorio</param>
    bool ValidarRutaDirectorio(string ruta);

    /// <summary>
    /// Obtiene el nombre del archivo DBF según su tipo
    /// </summary>
    /// <param name="tipo">Tipo de archivo DBF</param>
    string ObtenerNombreArchivo(TipoArchivoDbf tipo);

    /// <summary>
    /// Restablece la configuración a valores predeterminados
    /// </summary>
    Task RestablecerConfiguracionAsync();
}
