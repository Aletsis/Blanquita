using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class PrinterTests
{
    [Fact]
    public void Constructor_Initialization_ShouldWork()
    {
        var printer = new Printer
        {
            Name = "Cocina",
            IpAddress = "192.168.1.50",
            Port = 9100,
            IsActive = true
        };

        Assert.Equal("Cocina", printer.Name);
        Assert.Equal("192.168.1.50", printer.IpAddress);
        Assert.Equal(9100, printer.Port);
        Assert.True(printer.IsActive);
    }

    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var printer = new Printer();
        Assert.Equal(string.Empty, printer.Name);
        Assert.Equal(string.Empty, printer.IpAddress);
        Assert.Equal(0, printer.Port);
        Assert.True(printer.IsActive); 
    }
}
