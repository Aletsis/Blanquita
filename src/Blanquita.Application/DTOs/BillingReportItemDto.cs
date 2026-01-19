namespace Blanquita.Application.DTOs;

public record BillingReportItemDto
{
    [System.ComponentModel.DisplayName("Fecha")]
    public DateTime FechaEmision { get; init; }
    [System.ComponentModel.DisplayName("Hora")]
    public string HoraEmision { get; init; } = string.Empty;
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    public string Rfc { get; init; } = string.Empty;
    public string RazonSocial { get; init; } = string.Empty;
    
    // De MGW10008
    public decimal Neto { get; init; }
    
    [System.ComponentModel.DisplayName("IVA")]
    public decimal Impuesto1 { get; init; }
    
    public decimal Total { get; init; }
    [System.ComponentModel.DisplayName("Can")]
    public int Cancelado { get; init; }
    [System.ComponentModel.DisplayName("Edo")]
    public int Estado { get; init; }
    public int Entregado { get; init; }
    [System.ComponentModel.DisplayName("Can")]
    public string Cautusba01 { get; init; } = string.Empty;
    public string Uuid { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    
    [System.ComponentModel.DisplayName("Serie Nota")]
    public string TextoExtra3 { get; init; } = string.Empty;
    
    [System.ComponentModel.DisplayName("Folio Nota")]
    public decimal ImporteExtra3 { get; init; }
    
    // Internal use for joining
    public string IdDocumento { get; init; } = string.Empty;
}

