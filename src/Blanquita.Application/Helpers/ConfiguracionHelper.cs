using Blanquita.Application.DTOs;
using Blanquita.Domain.Enums;

namespace Blanquita.Application.Helpers;

/// <summary>
/// Helper para operaciones relacionadas con ConfiguracionDto
/// Separa la lógica de mapeo de los DTOs puros
/// </summary>
public static class ConfiguracionHelper
{
    /// <summary>
    /// Obtiene la ruta de un archivo DBF específico desde la configuración
    /// </summary>
    /// <param name="configuracion">DTO de configuración</param>
    /// <param name="tipo">Tipo de archivo DBF</param>
    /// <returns>Ruta del archivo o string vacío si no se encuentra</returns>
    public static string ObtenerRutaPorTipo(this ConfiguracionDto configuracion, TipoArchivoDbf tipo)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

        return tipo switch
        {
            TipoArchivoDbf.Pos10041 => configuracion.Pos10041Path,
            TipoArchivoDbf.Pos10042 => configuracion.Pos10042Path,
            TipoArchivoDbf.Mgw10008 => configuracion.Mgw10008Path,
            TipoArchivoDbf.Mgw10005 => configuracion.Mgw10005Path,
            TipoArchivoDbf.Mgw10045 => configuracion.Mgw10045Path,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Establece la ruta de un archivo DBF específico en la configuración
    /// </summary>
    /// <param name="configuracion">DTO de configuración</param>
    /// <param name="tipo">Tipo de archivo DBF</param>
    /// <param name="ruta">Nueva ruta del archivo</param>
    public static void EstablecerRutaPorTipo(
        this ConfiguracionDto configuracion, 
        TipoArchivoDbf tipo, 
        string ruta)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        ArgumentNullException.ThrowIfNull(ruta);

        switch (tipo)
        {
            case TipoArchivoDbf.Pos10041:
                configuracion.Pos10041Path = ruta;
                break;
            case TipoArchivoDbf.Pos10042:
                configuracion.Pos10042Path = ruta;
                break;
            case TipoArchivoDbf.Mgw10008:
                configuracion.Mgw10008Path = ruta;
                break;
            case TipoArchivoDbf.Mgw10005:
                configuracion.Mgw10005Path = ruta;
                break;
            case TipoArchivoDbf.Mgw10045:
                configuracion.Mgw10045Path = ruta;
                break;
        }
    }

    /// <summary>
    /// Obtiene el nombre del archivo DBF según su tipo
    /// </summary>
    /// <param name="tipo">Tipo de archivo DBF</param>
    /// <returns>Nombre del archivo con extensión</returns>
    public static string ObtenerNombreArchivoPorTipo(TipoArchivoDbf tipo)
    {
        return tipo switch
        {
            TipoArchivoDbf.Pos10041 => "POS10041.DBF",
            TipoArchivoDbf.Pos10042 => "POS10042.DBF",
            TipoArchivoDbf.Mgw10008 => "MGW10008.DBF",
            TipoArchivoDbf.Mgw10005 => "MGW10005.DBF",
            TipoArchivoDbf.Mgw10045 => "MGW10045.DBF",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Valida que todas las rutas de archivos DBF estén configuradas
    /// </summary>
    /// <param name="configuracion">DTO de configuración</param>
    /// <returns>True si todas las rutas están configuradas, False en caso contrario</returns>
    public static bool TieneTodasLasRutasConfiguradas(this ConfiguracionDto configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

        return !string.IsNullOrWhiteSpace(configuracion.Pos10041Path) &&
               !string.IsNullOrWhiteSpace(configuracion.Pos10042Path) &&
               !string.IsNullOrWhiteSpace(configuracion.Mgw10008Path) &&
               !string.IsNullOrWhiteSpace(configuracion.Mgw10005Path) &&
               !string.IsNullOrWhiteSpace(configuracion.Mgw10045Path);
    }

    /// <summary>
    /// Obtiene las rutas que faltan por configurar
    /// </summary>
    /// <param name="configuracion">DTO de configuración</param>
    /// <returns>Lista de tipos de archivo que faltan por configurar</returns>
    public static IEnumerable<TipoArchivoDbf> ObtenerRutasFaltantes(this ConfiguracionDto configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

        var faltantes = new List<TipoArchivoDbf>();

        if (string.IsNullOrWhiteSpace(configuracion.Pos10041Path))
            faltantes.Add(TipoArchivoDbf.Pos10041);

        if (string.IsNullOrWhiteSpace(configuracion.Pos10042Path))
            faltantes.Add(TipoArchivoDbf.Pos10042);

        if (string.IsNullOrWhiteSpace(configuracion.Mgw10008Path))
            faltantes.Add(TipoArchivoDbf.Mgw10008);

        if (string.IsNullOrWhiteSpace(configuracion.Mgw10005Path))
            faltantes.Add(TipoArchivoDbf.Mgw10005);

        if (string.IsNullOrWhiteSpace(configuracion.Mgw10045Path))
            faltantes.Add(TipoArchivoDbf.Mgw10045);

        return faltantes;
    }

    /// <summary>
    /// Obtiene las columnas esperadas para cada tipo de archivo DBF.
    /// </summary>
    /// <param name="tipo">Tipo de archivo DBF</param>
    /// <returns>Lista de nombres de columnas esperadas</returns>
    public static List<string> ObtenerColumnasEsperadas(TipoArchivoDbf tipo)
    {
        return tipo switch
        {
            TipoArchivoDbf.Pos10041 => new List<string> { "CIDCAJA", "CSERIENOTA" },
            TipoArchivoDbf.Pos10042 => new List<string> { "CFECHACOR", "CIDCAJA", "CFACTURA", "CDEVOLUCIO" },
            TipoArchivoDbf.Mgw10008 => new List<string> { "CFECHA", "CIDDOCUM02", "CTOTAL", "CSERIEDO01", "CFOLIO", "CTEXTOEX03", "CNETO", "CIMPUESTO1", "CCANCELADO", "CIMPORTE03" },
            TipoArchivoDbf.Mgw10005 => new List<string> { "CCODIGOP01", "CNOMBREP01", "CPRECIO1", "CIMPUESTO1" },
            TipoArchivoDbf.Mgw10045 => new List<string> { "Cfechaemi", "Choraemi", "Cserie", "Cfolio", "Crfc", "Crazon", "Cestado", "Centregado", "Cautusba01", "Cuuid", "Ciddocto" },
            _ => new List<string>()
        };
    }
}
