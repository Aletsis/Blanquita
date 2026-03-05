namespace Blanquita.Application.DTOs;

public record ReturnReportItemDto
{
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    public decimal Neto { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
}
