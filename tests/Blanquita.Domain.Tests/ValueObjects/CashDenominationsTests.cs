using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Tests.ValueObjects;

public class CashDenominationsTests
{
    [Fact]
    public void Create_ValidDenominations_ShouldCreateObject()
    {
        // Arrange & Act
        var denominations = CashDenominations.Create(5, 10, 15, 20, 25, 30);

        // Assert
        Assert.NotNull(denominations);
        Assert.Equal(5, denominations.Thousands);
        Assert.Equal(10, denominations.FiveHundreds);
        Assert.Equal(15, denominations.TwoHundreds);
        Assert.Equal(20, denominations.Hundreds);
        Assert.Equal(25, denominations.Fifties);
        Assert.Equal(30, denominations.Twenties);
    }

    [Fact]
    public void Create_NegativeDenomination_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CashDenominations.Create(-1, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void CalculateTotal_ValidDenominations_ShouldReturnCorrectTotal()
    {
        // Arrange
        // 2 x 1000 = 2000
        // 3 x 500 = 1500
        // 4 x 200 = 800
        // 5 x 100 = 500
        // 6 x 50 = 300
        // 7 x 20 = 140
        // Total = 5240
        var denominations = CashDenominations.Create(2, 3, 4, 5, 6, 7);

        // Act
        var total = denominations.CalculateTotal();

        // Assert
        Assert.Equal(5240m, total.Amount);
    }

    [Fact]
    public void CalculateTotal_ZeroDenominations_ShouldReturnZero()
    {
        // Arrange
        var denominations = CashDenominations.Zero;

        // Act
        var total = denominations.CalculateTotal();

        // Assert
        Assert.Equal(0m, total.Amount);
    }

    [Fact]
    public void Zero_ShouldReturnAllZeroDenominations()
    {
        // Act
        var zero = CashDenominations.Zero;

        // Assert
        Assert.Equal(0, zero.Thousands);
        Assert.Equal(0, zero.FiveHundreds);
        Assert.Equal(0, zero.TwoHundreds);
        Assert.Equal(0, zero.Hundreds);
        Assert.Equal(0, zero.Fifties);
        Assert.Equal(0, zero.Twenties);
    }
}
