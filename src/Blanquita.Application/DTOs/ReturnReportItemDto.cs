using System.ComponentModel;

namespace Blanquita.Application.DTOs;

public record ReturnReportItemDto
{
    public DateTime Fecha { get; init; }
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    [DisplayName("Referencia")]
    public string Referencia { get; init; } = string.Empty;
    public decimal Neto { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
}

