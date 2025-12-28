using Blanquita.Domain.Enums;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para la configuración del sistema
/// </summary>
public class ConfiguracionDto
{
    /// <summary>
    /// Ruta del archivo POS10041.DBF
    /// </summary>
    public string Pos10041Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo POS10042.DBF
    /// </summary>
    public string Pos10042Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo MGW10008.DBF
    /// </summary>
    public string Mgw10008Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo MGW10005.DBF
    /// </summary>
    public string Mgw10005Path { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la impresora principal
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;

    /// <summary>
    /// Dirección IP de la impresora principal
    /// </summary>
    public string PrinterIp { get; set; } = string.Empty;

    /// <summary>
    /// Puerto de la impresora principal
    /// </summary>
    public int PrinterPort { get; set; }

    /// <summary>
    /// Nombre de la impresora secundaria
    /// </summary>
    public string Printer2Name { get; set; } = string.Empty;

    /// <summary>
    /// Dirección IP de la impresora secundaria
    /// </summary>
    public string Printer2Ip { get; set; } = string.Empty;

    /// <summary>
    /// Puerto de la impresora secundaria
    /// </summary>
    public int Printer2Port { get; set; }

    /// <summary>
    /// Obtiene la ruta de un archivo DBF específico
    /// </summary>
    public string ObtenerRutaPorTipo(TipoArchivoDbf tipo)
    {
        return tipo switch
        {
            TipoArchivoDbf.Pos10041 => Pos10041Path,
            TipoArchivoDbf.Pos10042 => Pos10042Path,
            TipoArchivoDbf.Mgw10008 => Mgw10008Path,
            TipoArchivoDbf.Mgw10005 => Mgw10005Path,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Establece la ruta de un archivo DBF específico
    /// </summary>
    public void EstablecerRutaPorTipo(TipoArchivoDbf tipo, string ruta)
    {
        switch (tipo)
        {
            case TipoArchivoDbf.Pos10041:
                Pos10041Path = ruta;
                break;
            case TipoArchivoDbf.Pos10042:
                Pos10042Path = ruta;
                break;
            case TipoArchivoDbf.Mgw10008:
                Mgw10008Path = ruta;
                break;
            case TipoArchivoDbf.Mgw10005:
                Mgw10005Path = ruta;
                break;
        }
    }
}
