using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class PrinterConfigurationTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateConfig()
    {
        var config = PrinterConfiguration.Create("192.168.1.1", 9100);
        Assert.Equal("192.168.1.1", config.IpAddress);
        Assert.Equal(9100, config.Port);
    }

    [Fact]
    public void Create_InvalidIp_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => PrinterConfiguration.Create("", 9100));
    }

    [Fact]
    public void Create_InvalidPort_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => PrinterConfiguration.Create("127.0.0.1", 0));
        Assert.Throws<ArgumentException>(() => PrinterConfiguration.Create("127.0.0.1", -1));
    }

    [Fact]
    public void Equality_ShouldWork()
    {
        var c1 = PrinterConfiguration.Create("1.1.1.1", 80);
        var c2 = PrinterConfiguration.Create("1.1.1.1", 80);
        var c3 = PrinterConfiguration.Create("1.1.1.1", 81);
        
        Assert.Equal(c1, c2);
        Assert.NotEqual(c1, c3);
    }
}
