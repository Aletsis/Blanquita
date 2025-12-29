using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

/// <summary>
/// Mapper para convertir registros DBF a CashCutDto.
/// </summary>
public static class FoxProCashCutMapper
{
    /// <summary>
    /// Mapea un registro del reader a CashCutDto.
    /// </summary>
    public static CashCutDto MapToDto(IFoxProDataReader reader, string cashRegisterName, int branchId)
    {
        return new CashCutDto
        {
            CashRegisterId = reader.GetInt32Safe("CIDCAJA"),
            CashRegisterName = cashRegisterName,
            CutDateTime = reader.GetDateTimeSafe("CFECHACOR"),
            BranchName = $"Branch {branchId}",
            RawInvoices = reader.GetStringSafe("CFACTURA"),
            RawReturns = reader.GetStringSafe("CDEVOLUCIO")
        };
    }
}
