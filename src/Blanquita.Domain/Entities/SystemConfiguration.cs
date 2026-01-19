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
}
