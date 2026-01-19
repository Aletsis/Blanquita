using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

/// <summary>
/// Mapper para convertir registros DBF a ProductDto.
/// </summary>
public static class FoxProProductMapper
{
    /// <summary>
    /// Mapea un registro del reader a ProductDto.
    /// </summary>
    public static ProductDto MapToDto(IFoxProDataReader reader)
    {
        return new ProductDto
        {
            Code = reader.GetStringSafe("CCODIGOP01"),
            Name = reader.GetStringSafe("CNOMBREP01"),
            BasePrice = reader.GetDecimalSafe("CPRECIO1"),
            TaxRate = reader.GetDecimalSafe("CIMPUESTO1")
        };
    }

    /// <summary>
    /// Mapea un registro del reader a ProductSearchDto.
    /// </summary>
    public static ProductSearchDto MapToSearchDto(IFoxProDataReader reader)
    {
        return new ProductSearchDto
        {
            Code = reader.GetStringSafe("CCODIGOP01"),
            Name = reader.GetStringSafe("CNOMBREP01"),
            Price = reader.GetDecimalSafe("CPRECIO1"),
            Tax = reader.GetDecimalSafe("CIMPUESTO1"),
            AlternateCode = reader.GetStringSafe("CCODALTERN"),
            AlternateName = reader.GetStringSafe("CNOMALTERN"),
            SatKey = reader.GetStringSafe("CCLAVESAT"),
            Plu = reader.GetStringSafe("CTEXTOEX01")
        };
    }
}
