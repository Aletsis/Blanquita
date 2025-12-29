using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class SupervisorTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateSupervisor()
    {
        var sup = Supervisor.Create("Juan", 1, true);
        
        Assert.NotNull(sup);
        Assert.Equal("Juan", sup.Name);
        Assert.Equal(1, sup.BranchId.Value);
        Assert.True(sup.IsActive);
    }

    [Fact]
    public void Create_InvalidName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => Supervisor.Create("", 1));
    }

    [Fact]
    public void UpdateName_ValidName_ShouldUpdate()
    {
        var sup = Supervisor.Create("Juan", 1);
        sup.UpdateName("Pedro");
        Assert.Equal("Pedro", sup.Name);
    }

    [Fact]
    public void UpdateName_InvalidName_ShouldThrowException()
    {
        var sup = Supervisor.Create("Juan", 1);
        Assert.Throws<ArgumentException>(() => sup.UpdateName(""));
    }

    [Fact]
    public void UpdateBranch_ShouldUpdateBranchId()
    {
        var sup = Supervisor.Create("Juan", 1);
        sup.UpdateBranch(2);
        Assert.Equal(2, sup.BranchId.Value);
    }

    [Fact]
    public void ActivateDeactivate_ShouldChangeStatus()
    {
        var sup = Supervisor.Create("Juan", 1, true);
        
        sup.Deactivate();
        Assert.False(sup.IsActive);
        
        sup.Activate();
        Assert.True(sup.IsActive);
    }
}
