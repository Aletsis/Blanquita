using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class CashCollectionMapper
{
    public static CashCollectionDto ToDto(this CashCollection cashCollection)
    {
        return new CashCollectionDto
        {
            Id = cashCollection.Id,
            Thousands = cashCollection.Denominations.Thousands,
            FiveHundreds = cashCollection.Denominations.FiveHundreds,
            TwoHundreds = cashCollection.Denominations.TwoHundreds,
            Hundreds = cashCollection.Denominations.Hundreds,
            Fifties = cashCollection.Denominations.Fifties,
            Twenties = cashCollection.Denominations.Twenties,
            TotalAmount = cashCollection.GetTotalAmount(),
            CashRegisterName = cashCollection.CashRegisterName,
            CashierName = cashCollection.CashierName,
            SupervisorName = cashCollection.SupervisorName,
            CollectionDateTime = cashCollection.CollectionDateTime,
            Folio = cashCollection.Folio,
            IsForCashCut = cashCollection.IsForCashCut
        };
    }
}
