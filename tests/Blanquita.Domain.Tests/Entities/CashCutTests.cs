using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class CashCutTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateCashCut()
    {
        // Arrange
        var register = "Caja 1";
        var supervisor = "Pedro";
        var cashier = "Juan";
        var branch = "Sucursal 1";

        // Act
        var cashCut = CashCut.Create(1, 2, 3, 4, 5, 6, 100.50m, 200.00m, register, supervisor, cashier, branch);

        // Assert
        Assert.NotNull(cashCut);
        Assert.Equal(register, cashCut.CashRegisterName);
        Assert.Equal(supervisor, cashCut.SupervisorName);
        Assert.Equal(cashier, cashCut.CashierName);
        Assert.Equal(branch, cashCut.BranchName);
        Assert.NotEqual(default, cashCut.CutDateTime);

        // Assert totals
        Assert.Equal(1, cashCut.Totals.TotalThousands);
        Assert.Equal(100.50m, cashCut.Totals.TotalSlips);
        Assert.Equal(200.00m, cashCut.Totals.TotalCards);
    }

    [Fact]
    public void Create_InvalidArguments_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCut.Create(0,0,0,0,0,0,0,0, "", "Sup", "Cash", "Branch"));
        Assert.Throws<ArgumentException>(() => CashCut.Create(0,0,0,0,0,0,0,0, "Reg", "", "Cash", "Branch"));
        Assert.Throws<ArgumentException>(() => CashCut.Create(0,0,0,0,0,0,0,0, "Reg", "Sup", "", "Branch"));
        Assert.Throws<ArgumentException>(() => CashCut.Create(0,0,0,0,0,0,0,0, "Reg", "Sup", "Cash", ""));
    }

    [Fact]
    public void GetGrandTotal_ShouldCalculateCorrectly()
    {
        // Grand Total should be the sum of collections (denominations)
        // Collections: 1*1000 = 1000
        // Slips: 50
        // Cards: 100
        // GetGrandTotal should return sum of collections (1000)
        
        var cashCut = CashCut.Create(1, 0, 0, 0, 0, 0, 50m, 100m, "R", "S", "C", "B");
        
        var grandTotal = cashCut.GetGrandTotal();
        
        Assert.Equal(1000m, grandTotal.Amount);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenTotalGreaterThanZero()
    {
        // IsValid checks if GrandTotal (sum of collections) > 0
        var cashCut = CashCut.Create(1, 0, 0, 0, 0, 0, 100m, 0, "R", "S", "C", "B");
        Assert.True(cashCut.IsValid());
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTotalIsZero()
    {
        var cashCut = CashCut.Create(0, 0, 0, 0, 0, 0, 0, 0, "R", "S", "C", "B");
        Assert.False(cashCut.IsValid());
    }
}
