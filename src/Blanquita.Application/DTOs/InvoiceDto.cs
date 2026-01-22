namespace Blanquita.Application.DTOs;

public class InvoiceDto
{
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public DateTime Fecha { get; set; }
    public string FileName { get; set; } = string.Empty;
}
