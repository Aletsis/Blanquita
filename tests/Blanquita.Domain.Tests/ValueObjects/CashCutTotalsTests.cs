using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class CashCutTotalsTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateCashCutTotals()
    {
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 100m, 150m, 50m);
        
        Assert.Equal(1, totals.TotalThousands);
        Assert.Equal(2, totals.TotalFiveHundreds);
        Assert.Equal(3, totals.TotalTwoHundreds);
        Assert.Equal(4, totals.TotalHundreds);
        Assert.Equal(5, totals.TotalFifties);
        Assert.Equal(6, totals.TotalTwenties);
        Assert.Equal(100m, totals.TotalSlips.Amount);
        Assert.Equal(150m, totals.TotalBanbajio.Amount);
        Assert.Equal(50m, totals.TotalBanregio.Amount);
    }

    [Fact]
    public void Create_NegativeValues_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCutTotals.Create(-1, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void CalculateCollectionsTotal_ShouldSumCashDenominations()
    {
        // 1*1000 + 2*500 + 3*200 + 4*100 + 5*50 + 6*20 = 3370
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 0m, 0m, 0m);
        
        Assert.Equal(3370m, totals.CalculateCollectionsTotal().Amount);
    }

    [Fact]
    public void CalculateCashToDeliver_ShouldCalculateCorrectly()
    {
        // Cash To Deliver = Total Tira (Slips) - Collections (Cash) - Cards (Banbajio + Banregio)
        // Let's say:
        // Slips (Total expected) = 5000
        // Collections (Cash counted) = 3370
        // Banbajio = 600, Banregio = 400, Total Cards = 1000
        // Result = 5000 - 3370 - 1000 = 630
        
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 5000m, 600m, 400m);
        // Collections = 3370
        
        var result = totals.CalculateCashToDeliver();
        
        Assert.Equal(5000m - 3370m - 1000m, result.Amount); // 630
    }

    [Fact]
    public void CalculateGrandTotal_ShouldReturnCollectionsTotal()
    {
        // Grand Total should be the total collections (sum of denominations)
        // When there are no collections, it should be 0
        var totals = CashCutTotals.Create(0, 0, 0, 0, 0, 0, 200000m, 30m, 15m);
        
        var grandTotal = totals.CalculateGrandTotal();
        
        Assert.Equal(0m, grandTotal.Amount); // No collections = 0
    }

    [Fact]
    public void CalculateGrandTotal_WithCollectionsAndSlips_ShouldReturnCollectionsTotal()
    {
        // Grand total should be the sum of collections (denominations), not TotalSlips
        // 1*1000 + 2*500 + 3*200 + 4*100 + 5*50 + 6*20 = 3370
        var totals = CashCutTotals.Create(1, 2, 3, 4, 5, 6, 5000m, 600m, 400m);
        
        var grandTotal = totals.CalculateGrandTotal();
        
        Assert.Equal(3370m, grandTotal.Amount); // Sum of denominations
    }
}
