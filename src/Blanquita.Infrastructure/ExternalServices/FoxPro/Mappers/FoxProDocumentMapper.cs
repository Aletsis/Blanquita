using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

/// <summary>
/// Mapper para convertir registros DBF a DocumentDto.
/// </summary>
public static class FoxProDocumentMapper
{
    /// <summary>
    /// Mapea un registro del reader a DocumentDto.
    /// </summary>
    public static DocumentDto MapToDto(IFoxProDataReader reader)
    {
        var documentNumber = reader.GetStringSafe("CIDDOCUM02");
        
        return new DocumentDto
        {
            DocumentNumber = documentNumber,
            IdDocumento = documentNumber,
            Date = reader.GetDateTimeSafe("CFECHA"),
            Total = reader.GetDecimalSafe("CTOTAL"),
            Serie = reader.GetStringSafe("CSERIEDO01"),
            Folio = reader.GetStringSafe("CFOLIO"),
            CajaTexto = reader.GetStringSafe("CTEXTOEX03"),
            CustomerName = string.Empty
        };
    }
}
