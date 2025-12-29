namespace Blanquita.Application.DTOs;

public class LabelElementDto
{
    public int Id { get; set; }
    public int LabelDesignId { get; set; }
    public string ElementType { get; set; } = "Text"; // "Text", "Barcode"
    public decimal XMm { get; set; }
    public decimal YMm { get; set; }
    public string Content { get; set; } = string.Empty;
    public int FontSize { get; set; } = 30;
    public decimal? HeightMm { get; set; }
    public int? BarWidth { get; set; }
}
