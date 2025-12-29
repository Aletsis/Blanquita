using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class SystemConfigurationTests
{
    [Fact]
    public void Properties_ShouldAllowGetSet()
    {
        var config = new SystemConfiguration
        {
            Pos10041Path = "path1",
            Pos10042Path = "path2",
            Mgw10008Path = "path3",
            Mgw10005Path = "path4",
            PrinterName = "Printer1",
            PrinterIp = "1.2.3.4",
            PrinterPort = 9100,
            Printer2Name = "Printer2",
            Printer2Ip = "5.6.7.8",
            Printer2Port = 9101
        };

        Assert.Equal("path1", config.Pos10041Path);
        Assert.Equal("path2", config.Pos10042Path);
        Assert.Equal("path3", config.Mgw10008Path);
        Assert.Equal("path4", config.Mgw10005Path);
        Assert.Equal("Printer1", config.PrinterName);
        Assert.Equal("1.2.3.4", config.PrinterIp);
        Assert.Equal(9100, config.PrinterPort);
        Assert.Equal("Printer2", config.Printer2Name);
        Assert.Equal("5.6.7.8", config.Printer2Ip);
        Assert.Equal(9101, config.Printer2Port);
    }
}
