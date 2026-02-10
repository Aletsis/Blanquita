using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

/// <summary>
/// Mapper to convert DBF records to ReturnDto.
/// </summary>
public static class FoxProReturnMapper
{
    /// <summary>
    /// Maps a reader record to ReturnDto.
    /// </summary>
    public static ReturnDto MapToDto(IFoxProDataReader reader)
    {
        return new ReturnDto
        {
            IdDocument = reader.GetStringSafe("CIDDOCUM01"),
            Series = reader.GetStringSafe("CSERIEDO01"),
            Folio = reader.GetStringSafe("CFOLIO"),
            Date = reader.GetDateTimeSafe("CFECHA"),
            Time = reader.GetStringSafe("CHORA"),
            Net = reader.GetDecimalSafe("CNETO"),
            Tax = reader.GetDecimalSafe("CIMPUESTO"),
            Total = reader.GetDecimalSafe("CTOTAL")
        };
    }
}
