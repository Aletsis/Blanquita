using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class CashCutTotalsTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateCashCutTotals()
    {
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 100m, 200m);
        
        Assert.Equal(1, totals.TotalThousands);
        Assert.Equal(2, totals.TotalFiveHundreds);
        Assert.Equal(3, totals.TotalTwoHundreds);
        Assert.Equal(4, totals.TotalHundreds);
        Assert.Equal(5, totals.TotalFifties);
        Assert.Equal(6, totals.TotalTwenties);
        Assert.Equal(100m, totals.TotalSlips.Amount);
        Assert.Equal(200m, totals.TotalCards.Amount);
    }

    [Fact]
    public void Create_NegativeValues_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCutTotals.Create(-1, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void CalculateCollectionsTotal_ShouldSumCashDenominations()
    {
        // 1*1000 + 2*500 + 3*200 + 4*100 + 5*50 + 6*20 = 3370
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 0m, 0m);
        
        Assert.Equal(3370m, totals.CalculateCollectionsTotal().Amount);
    }

    [Fact]
    public void CalculateCashToDeliver_ShouldCalculateCorrectly()
    {
        // Cash To Deliver = Total Tira (Slips) - Collections (Cash) - Cards
        // Let's say:
        // Slips (Total expected) = 5000
        // Collections (Cash counted) = 3370
        // Cards = 1000
        // Result = 5000 - 3370 - 1000 = 630 (Positive shortfall/surplus? Need to check business logic naming)
        // Usually CashToDeliver is what 'should' be remaining or is calculated based on system expectation vs reality.
        
        // Reading logic: 
        // var cashToDeliver = TotalSlips.Amount - collectionsTotal.Amount - TotalCards.Amount;
        
        // If Slips is "Revenue recorded by system" (e.g. Sales)
        // And Collections is "Cash picked up"
        // And Cards is "Card payments"
        
        // Then (Sales) - (Cash Picked Up) - (Card Payments) = Remaining Cash in Drawer?
        
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 5000m, 1000m);
        // Collections = 3370
        
        var result = totals.CalculateCashToDeliver();
        
        Assert.Equal(5000m - 3370m - 1000m, result.Amount); // 630
    }

    [Fact]
    public void CalculateGrandTotal_ShouldReturnTotalSlips()
    {
        // Grand Total should be the total sales (TotalSlips)
        // This allows cash cuts to be created even when there are no cash collections
        var totals = CashCutTotals.Create(0, 0, 0, 0, 0, 0, 200000m, 45m);
        
        var grandTotal = totals.CalculateGrandTotal();
        
        Assert.Equal(200000m, grandTotal.Amount);
    }

    [Fact]
    public void CalculateGrandTotal_WithCollectionsAndSlips_ShouldReturnTotalSlips()
    {
        // Even when there are collections, grand total should still be TotalSlips
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 5000m, 1000m);
        
        var grandTotal = totals.CalculateGrandTotal();
        
        Assert.Equal(5000m, grandTotal.Amount);
    }
}
