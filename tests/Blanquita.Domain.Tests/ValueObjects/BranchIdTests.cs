using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class BranchIdTests
{
    [Fact]
    public void Create_ValidId_ShouldCreate()
    {
        var branchId = BranchId.Create(1);
        Assert.Equal(1, branchId.Value);
    }

    [Fact]
    public void Create_InvalidId_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => BranchId.Create(0));
        Assert.Throws<ArgumentException>(() => BranchId.Create(-1));
    }

    [Fact]
    public void ImplicitConversion_ToInt_ShouldWork()
    {
        var branchId = BranchId.Create(5);
        int value = branchId;
        Assert.Equal(5, value);
    }

    [Fact]
    public void Equality_ShouldWork()
    {
        var bid1 = BranchId.Create(1);
        var bid2 = BranchId.Create(1);
        var bid3 = BranchId.Create(2);

        Assert.Equal(bid1, bid2);
        Assert.NotEqual(bid1, bid3);
    }
}
