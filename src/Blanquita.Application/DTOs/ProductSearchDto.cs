namespace Blanquita.Application.DTOs;

public class ProductSearchDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } // Cprecio1
    public decimal Tax { get; set; } // Cimpuesto1
    public string AlternateCode { get; set; } = string.Empty; // Ccodaltern
    public string AlternateName { get; set; } = string.Empty; // Cnomaltern
    public string SatKey { get; set; } = string.Empty; // Cclavesat
    public string Plu { get; set; } = string.Empty; // Ctextoex01 (Renamed to PLU)
}
