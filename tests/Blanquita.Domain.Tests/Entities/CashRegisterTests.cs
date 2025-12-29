using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class CashRegisterTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateRegister()
    {
        var reg = CashRegister.Create("Caja 1", "192.168.1.100", 9100, 1, true);
        
        Assert.NotNull(reg);
        Assert.Equal("Caja 1", reg.Name);
        Assert.Equal("192.168.1.100", reg.PrinterConfig.IpAddress);
        Assert.Equal(9100, reg.PrinterConfig.Port);
        Assert.Equal(1, reg.BranchId.Value);
        Assert.True(reg.IsLastRegister);
    }

    [Fact]
    public void Create_InvalidName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CashRegister.Create("", "127.0.0.1", 9100, 1));
    }

    [Fact]
    public void UpdateName_ShouldUpdate()
    {
        var reg = CashRegister.Create("Caja 1", "127.0.0.1", 9100, 1);
        reg.UpdateName("Caja 2");
        Assert.Equal("Caja 2", reg.Name);
    }

    [Fact]
    public void UpdatePrinterConfiguration_ShouldUpdate()
    {
        var reg = CashRegister.Create("Caja 1", "127.0.0.1", 9100, 1);
        reg.UpdatePrinterConfiguration("10.0.0.1", 8080);
        
        Assert.Equal("10.0.0.1", reg.PrinterConfig.IpAddress);
        Assert.Equal(8080, reg.PrinterConfig.Port);
    }

    [Fact]
    public void UpdateBranch_ShouldUpdate()
    {
        var reg = CashRegister.Create("Caja 1", "127.0.0.1", 9100, 1);
        reg.UpdateBranch(2);
        Assert.Equal(2, reg.BranchId.Value);
    }

    [Fact]
    public void SetUnsetLastRegister_ShouldUpdateStatus()
    {
        var reg = CashRegister.Create("Caja 1", "127.0.0.1", 9100, 1, false);
        
        reg.SetAsLastRegister();
        Assert.True(reg.IsLastRegister);
        
        reg.UnsetAsLastRegister();
        Assert.False(reg.IsLastRegister);
    }
}
