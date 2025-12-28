namespace Blanquita.Infrastructure.Models;

/// <summary>
/// Modelo de configuración de la aplicación para persistencia
/// </summary>
public class AppConfiguration
{
    public string Pos10041Path { get; set; } = string.Empty;
    public string Pos10042Path { get; set; } = string.Empty;
    public string Mgw10008Path { get; set; } = string.Empty;
    public string Mgw10005Path { get; set; } = string.Empty;

    public string PrinterName { get; set; } = string.Empty;
    public string PrinterIp { get; set; } = string.Empty;
    public string PrinterPort { get; set; } = string.Empty;
    public string Printer2Name { get; set; } = string.Empty;
    public string Printer2Ip { get; set; } = string.Empty;
    public string Printer2Port { get; set; } = string.Empty;
}
