using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.Printing;
using Xunit;

namespace Blanquita.Infrastructure.Tests.ExternalServices.Printing;

public class PrinterCommandBuilderTests
{
    private readonly PrinterCommandBuilder _builder;

    public PrinterCommandBuilderTests()
    {
        _builder = new PrinterCommandBuilder();
    }

    [Fact]
    public void InitializePrinter_ShouldReturnCorrectBytes()
    {
        var result = _builder.InitializePrinter();

        Assert.Equal(new byte[] { 0x1B, 0x40 }, result);
    }

    [Fact]
    public void CutPaper_ShouldReturnCorrectBytes()
    {
        var result = _builder.CutPaper();

        Assert.Equal(new byte[] { 0x1D, 0x56, 66, 0 }, result);
    }

    [Fact]
    public void Text_ShouldConvertToASCII()
    {
        var result = _builder.Text("Test");

        Assert.NotEmpty(result);
        Assert.Equal(4, result.Length);
    }

    [Fact]
    public void BoldOn_ShouldReturnCorrectBytes()
    {
        var result = _builder.BoldOn();

        Assert.Equal(new byte[] { 0x1B, 0x45, 1 }, result);
    }

    [Fact]
    public void BoldOff_ShouldReturnCorrectBytes()
    {
        var result = _builder.BoldOff();

        Assert.Equal(new byte[] { 0x1B, 0x45, 0 }, result);
    }

    [Fact]
    public void AlignLeft_ShouldReturnCorrectBytes()
    {
        var result = _builder.AlignLeft();

        Assert.Equal(new byte[] { 0x1B, 0x61, 0 }, result);
    }

    [Fact]
    public void AlignCenter_ShouldReturnCorrectBytes()
    {
        var result = _builder.AlignCenter();

        Assert.Equal(new byte[] { 0x1B, 0x61, 1 }, result);
    }

    [Fact]
    public void AlignRight_ShouldReturnCorrectBytes()
    {
        var result = _builder.AlignRight();

        Assert.Equal(new byte[] { 0x1B, 0x61, 2 }, result);
    }

    [Fact]
    public void BuildCashCollectionTicket_ShouldGenerateCommands()
    {
        var collection = new CashCollectionDto
        {
            Thousands = 1,
            FiveHundreds = 2,
            TwoHundreds = 0,
            Hundreds = 0,
            Fifties = 0,
            Twenties = 0,
            CashRegisterName = "Caja 1",
            CashierName = "Juan",
            SupervisorName = "Pedro",
            CollectionDateTime = DateTime.Now,
            Folio = 123
        };

        var result = _builder.BuildCashCollectionTicket(collection);

        Assert.NotEmpty(result);
        // Should contain initialization, text, and cut commands
        Assert.Contains((byte)0x1B, result); // ESC commands
        Assert.Contains((byte)0x1D, result); // GS commands (cut)
    }

    [Fact]
    public void BuildCashCutTicket_ShouldGenerateCommands()
    {
        var cashCut = new CashCutDto
        {
            TotalThousands = 5,
            TotalFiveHundreds = 10,
            TotalTwoHundreds = 0,
            TotalHundreds = 0,
            TotalFifties = 0,
            TotalTwenties = 0,
            TotalSlips = 10000m,
            TotalCards = 500m,
            GrandTotal = 10000m,
            CashRegisterName = "Caja 1",
            SupervisorName = "Pedro",
            CashierName = "Juan",
            BranchName = "Himno Nacional",
            CutDateTime = DateTime.Now
        };

        var result = _builder.BuildCashCutTicket(cashCut);

        Assert.NotEmpty(result);
        Assert.Contains((byte)0x1B, result);
        Assert.Contains((byte)0x1D, result);
    }

    [Fact]
    public void BuildCustomTicket_ShouldGenerateCommands()
    {
        var ticket = new TicketDto
        {
            Title = "Test Ticket",
            Lines = new List<string> { "Line 1", "Line 2", "Line 3" }
        };

        var result = _builder.BuildCustomTicket(ticket);

        Assert.NotEmpty(result);
        Assert.Contains((byte)0x1B, result);
        Assert.Contains((byte)0x1D, result);
    }

    [Fact]
    public void BuildCashCollectionTicket_ShouldHandleZeroQuantities()
    {
        var collection = new CashCollectionDto
        {
            Thousands = 0,
            FiveHundreds = 0,
            TwoHundreds = 0,
            Hundreds = 0,
            Fifties = 0,
            Twenties = 0,
            CashRegisterName = "Caja 1",
            CashierName = "Juan",
            SupervisorName = "Pedro",
            CollectionDateTime = DateTime.Now,
            Folio = 1
        };

        var result = _builder.BuildCashCollectionTicket(collection);

        Assert.NotEmpty(result);
        // Should still generate valid ticket structure
    }
}
