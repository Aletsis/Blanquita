namespace Blanquita.Application.DTOs;

public class ProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal TaxRate { get; set; }
    
    public decimal PriceWithTax => Math.Round(BasePrice * (1 + (TaxRate / 100m)), 2, MidpointRounding.AwayFromZero);
}
