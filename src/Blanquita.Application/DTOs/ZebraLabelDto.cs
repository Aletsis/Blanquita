namespace Blanquita.Application.DTOs;

public record ZebraLabelDto
{
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string PrinterIp { get; init; } = string.Empty;
    public int PrinterPort { get; init; }
    public int Quantity { get; init; } = 1;
}
