using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ValidAmount_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;

        // Act
        var money = Money.Create(amount);

        // Assert
        Assert.NotNull(money);
        Assert.Equal(amount, money.Amount);
    }

    [Fact]
    public void Create_NegativeAmount_ShouldThrowException()
    {
        // Arrange
        var amount = -10m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(amount));
    }

    [Fact]
    public void Add_TwoMoneyObjects_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Subtract_TwoMoneyObjects_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(30m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Multiply_MoneyByFactor_ShouldReturnProduct()
    {
        // Arrange
        var money = Money.Create(50m);
        var factor = 3m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void OperatorPlus_TwoMoneyObjects_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1 + money2;

        // Assert
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        // Act
        var zero = Money.Zero;

        // Assert
        Assert.Equal(0m, zero.Amount);
    }
}
