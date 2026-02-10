using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

/// <summary>
/// Mapper to convert DBF records to ReturnDetailDto.
/// </summary>
public static class FoxProReturnDetailMapper
{
    /// <summary>
    /// Maps a reader record to ReturnDetailDto.
    /// </summary>
    public static ReturnDetailDto MapToDto(IFoxProDataReader reader)
    {
        return new ReturnDetailDto
        {
            ProductId = reader.GetStringSafe("CIDPRODU01"),
            Units = reader.GetDoubleSafe("CUNIDADES"),
            Net = reader.GetDecimalSafe("CNETO"),
            Tax = reader.GetDecimalSafe("CIMPUESTO1"),
            Total = reader.GetDecimalSafe("CTOTAL")
        };
    }
}
