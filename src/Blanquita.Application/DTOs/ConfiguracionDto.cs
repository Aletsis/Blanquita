using Blanquita.Domain.Enums;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para la configuración del sistema
/// Objeto de transferencia puro sin lógica de negocio
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
    /// Ruta del archivo MGW10045.DBF
    /// </summary>
    public string Mgw10045Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo MGW10002.DBF
    /// </summary>
    public string Mgw10002Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo MGW10011.DBF
    /// </summary>
    public string Mgw10011Path { get; set; } = string.Empty;

    /// <summary>
    /// Ruta de la carpeta para facturas
    /// </summary>
    public string FacturasPath { get; set; } = string.Empty;

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
    /// Servidor SMTP
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Puerto SMTP
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// Usuario SMTP
    /// </summary>
    public string SmtpUser { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña SMTP
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Habilitar SSL para SMTP
    /// </summary>
    public bool SmtpEnableSsl { get; set; }

    /// <summary>
    /// Correo del remitente
    /// </summary>
    public string SmtpFromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del remitente
    /// </summary>
    public string SmtpFromName { get; set; } = string.Empty;

    /// <summary>
    /// Hora del día en la que se ejecutará el envío automático de facturas
    /// </summary>
    public TimeSpan? InvoiceJobExecutionTime { get; set; } = new TimeSpan(18, 0, 0);

    /// <summary>
    /// Correos para alertas y resúmenes (separados por coma)
    /// </summary>
    public string AlertEmails { get; set; } = string.Empty;
}
