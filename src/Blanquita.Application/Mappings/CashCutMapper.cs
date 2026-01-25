using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class CashCutMapper
{
    public static CashCutDto ToDto(this CashCut cashCut)
    {
        return new CashCutDto
        {
            Id = cashCut.Id,
            TotalThousands = cashCut.Totals.TotalThousands,
            TotalFiveHundreds = cashCut.Totals.TotalFiveHundreds,
            TotalTwoHundreds = cashCut.Totals.TotalTwoHundreds,
            TotalHundreds = cashCut.Totals.TotalHundreds,
            TotalFifties = cashCut.Totals.TotalFifties,
            TotalTwenties = cashCut.Totals.TotalTwenties,
            TotalSlips = cashCut.Totals.TotalSlips,
            TotalBanbajio = cashCut.Totals.TotalBanbajio,
            TotalBanregio = cashCut.Totals.TotalBanregio,
            CollectionsTotal = cashCut.Totals.CalculateCollectionsTotal(),
            CashToDeliver = cashCut.Totals.CalculateCashToDeliver(),
#pragma warning disable CS0618 // Type or member is obsolete
            GrandTotal = cashCut.GetGrandTotal(),
#pragma warning restore CS0618 // Type or member is obsolete
            CashRegisterName = cashCut.CashRegisterName,
            SupervisorName = cashCut.SupervisorName,
            CashierName = cashCut.CashierName,
            BranchName = cashCut.BranchName,
            CutDateTime = cashCut.CutDateTime
        };
    }

    public static CashCut ToEntity(this CreateCashCutDto dto)
    {
        return CashCut.Create(
            dto.TotalThousands, dto.TotalFiveHundreds, dto.TotalTwoHundreds,
            dto.TotalHundreds, dto.TotalFifties, dto.TotalTwenties,
            dto.TotalSlips, dto.TotalBanbajio, dto.TotalBanregio,
            dto.CashRegisterName, dto.SupervisorName, dto.CashierName, dto.BranchName);
    }
}
