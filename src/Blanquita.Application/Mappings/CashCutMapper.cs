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
            TotalCards = cashCut.Totals.TotalCards,
            GrandTotal = cashCut.GetGrandTotal(),
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
            dto.TotalSlips, dto.TotalCards,
            dto.CashRegisterName, dto.SupervisorName, dto.CashierName, dto.BranchName);
    }
}
