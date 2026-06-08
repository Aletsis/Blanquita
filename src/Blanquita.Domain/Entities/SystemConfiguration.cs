using System.ComponentModel.DataAnnotations;

namespace Blanquita.Domain.Entities;

public class SystemConfiguration : BaseEntity
{
    [MaxLength(500)]
    public string Pos10041Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Pos10042Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Mgw10008Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Mgw10005Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Mgw10045Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Mgw10002Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Mgw10011Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Pos10008Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Pos10010Path { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FacturasPath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PrinterName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PrinterIp { get; set; } = string.Empty;

    public int PrinterPort { get; set; }

    [MaxLength(200)]
    public string Printer2Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Printer2Ip { get; set; } = string.Empty;

    public int Printer2Port { get; set; }
    
    // SMTP Configuration
    [MaxLength(200)]
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    [MaxLength(200)]
    public string SmtpUser { get; set; } = string.Empty;
    [MaxLength(200)]
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpEnableSsl { get; set; }
    [MaxLength(200)]
    public string SmtpFromEmail { get; set; } = string.Empty;
    [MaxLength(200)]
    public string SmtpFromName { get; set; } = string.Empty;

    public TimeSpan? InvoiceJobExecutionTime { get; set; } = new TimeSpan(18, 0, 0); // Default 6 PM

    [MaxLength(500)]
    public string AlertEmails { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CommercialApiUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CommercialApiKey { get; set; } = string.Empty;

    [MaxLength(500)]
    public string WhatsAppServiceUrl { get; set; } = string.Empty;
}
