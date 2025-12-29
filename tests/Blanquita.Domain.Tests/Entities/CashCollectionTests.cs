using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class CashCollectionTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateCashCollection()
    {
        // Arrange
        var register = "Caja 1";
        var cashier = "Juan";
        var supervisor = "Pedro";
        var folio = 100;

        // Act
        var collection = CashCollection.Create(1, 2, 3, 4, 5, 6, register, cashier, supervisor, folio);

        // Assert
        Assert.NotNull(collection);
        Assert.Equal(register, collection.CashRegisterName);
        Assert.Equal(cashier, collection.CashierName);
        Assert.Equal(supervisor, collection.SupervisorName);
        Assert.Equal(folio, collection.Folio);
        Assert.False(collection.IsForCashCut);
        Assert.NotEqual(default, collection.CollectionDateTime);
        
        // Assert denominations
        Assert.Equal(1, collection.Denominations.Thousands);
        Assert.Equal(2, collection.Denominations.FiveHundreds);
        Assert.Equal(3, collection.Denominations.TwoHundreds);
        Assert.Equal(4, collection.Denominations.Hundreds);
        Assert.Equal(5, collection.Denominations.Fifties);
        Assert.Equal(6, collection.Denominations.Twenties);
    }

    [Fact]
    public void Create_InvalidRegisterName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCollection.Create(0,0,0,0,0,0, "", "Juan", "Pedro", 1));
    }

    [Fact]
    public void Create_InvalidCashierName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCollection.Create(0,0,0,0,0,0, "Caja 1", "", "Pedro", 1));
    }

    [Fact]
    public void Create_InvalidSupervisorName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCollection.Create(0,0,0,0,0,0, "Caja 1", "Juan", "", 1));
    }

    [Fact]
    public void Create_InvalidFolio_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashCollection.Create(0,0,0,0,0,0, "Caja 1", "Juan", "Pedro", 0));
        Assert.Throws<ArgumentException>(() => CashCollection.Create(0,0,0,0,0,0, "Caja 1", "Juan", "Pedro", -1));
    }

    [Fact]
    public void GetTotalAmount_ShouldCalculateCorrectly()
    {
        // 1*1000 + 2*500 + 3*200 + 4*100 + 5*50 + 6*20
        // 1000 + 1000 + 600 + 400 + 250 + 120 = 3370
        var collection = CashCollection.Create(1, 2, 3, 4, 5, 6, "Caja 1", "Juan", "Pedro", 100);

        var total = collection.GetTotalAmount();
        
        Assert.Equal(3370, total.Amount);
    }

    [Fact]
    public void MarkAsForCashCut_ShouldSetFlagToTrue()
    {
        var collection = CashCollection.Create(0,0,0,0,0,0, "Caja 1", "Juan", "Pedro", 100);
        
        collection.MarkAsForCashCut();
        
        Assert.True(collection.IsForCashCut);
    }
}
