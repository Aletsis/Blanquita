using Blanquita.Domain.Entities;
using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Tests.Entities;

public class CashierTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateCashier()
    {
        // Arrange
        var employeeNumber = 12345;
        var name = "Juan Pérez";
        var branchId = 1;

        // Act
        var cashier = Cashier.Create(employeeNumber, name, branchId);

        // Assert
        Assert.NotNull(cashier);
        Assert.Equal(employeeNumber, cashier.EmployeeNumber);
        Assert.Equal(name, cashier.Name);
        Assert.Equal(branchId, cashier.BranchId.Value);
        Assert.True(cashier.IsActive);
    }

    [Fact]
    public void Create_InvalidEmployeeNumber_ShouldThrowException()
    {
        // Arrange
        var employeeNumber = 0;
        var name = "Juan Pérez";
        var branchId = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cashier.Create(employeeNumber, name, branchId));
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        // Arrange
        var employeeNumber = 12345;
        var name = "";
        var branchId = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cashier.Create(employeeNumber, name, branchId));
    }

    [Fact]
    public void UpdateName_ValidName_ShouldUpdateName()
    {
        // Arrange
        var cashier = Cashier.Create(12345, "Juan Pérez", 1);
        var newName = "Juan Carlos Pérez";

        // Act
        cashier.UpdateName(newName);

        // Assert
        Assert.Equal(newName, cashier.Name);
    }

    [Fact]
    public void Deactivate_ActiveCashier_ShouldDeactivate()
    {
        // Arrange
        var cashier = Cashier.Create(12345, "Juan Pérez", 1);
        Assert.True(cashier.IsActive);

        // Act
        cashier.Deactivate();

        // Assert
        Assert.False(cashier.IsActive);
    }

    [Fact]
    public void Activate_InactiveCashier_ShouldActivate()
    {
        // Arrange
        var cashier = Cashier.Create(12345, "Juan Pérez", 1);
        cashier.Deactivate();
        Assert.False(cashier.IsActive);

        // Act
        cashier.Activate();

        // Assert
        Assert.True(cashier.IsActive);
    }
}
