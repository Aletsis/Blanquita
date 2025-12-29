using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class LabelElementTests
{
    [Fact]
    public void Create_ValidData_ShouldCreate()
    {
        var element = LabelElement.Create("Text", 10, 20, "Contenido", 30, null, null);
        
        Assert.Equal("Text", element.ElementType);
        Assert.Equal(10, element.XMm);
        Assert.Equal(20, element.YMm);
        Assert.Equal("Contenido", element.Content);
        Assert.Equal(30, element.FontSize);
        Assert.Null(element.HeightMm);
        Assert.Null(element.BarWidth);
    }

    [Fact]
    public void CreateText_ShouldSetDefaultsForText()
    {
        var element = LabelElement.CreateText(5, 5, "Hello", 25);
        
        Assert.Equal("Text", element.ElementType);
        Assert.Equal(5, element.XMm);
        Assert.Equal(5, element.YMm);
        Assert.Equal("Hello", element.Content);
        Assert.Equal(25, element.FontSize);
    }

    [Fact]
    public void CreateBarcode_ShouldSetDefaultsForBarcode()
    {
        var element = LabelElement.CreateBarcode(10, 10, "{Code}", 15, 3);
        
        Assert.Equal("Barcode", element.ElementType);
        Assert.Equal(10, element.XMm);
        Assert.Equal(10, element.YMm);
        Assert.Equal("{Code}", element.Content);
        Assert.Equal(15, element.HeightMm);
        Assert.Equal(3, element.BarWidth);
    }

    [Fact]
    public void UpdatePosition_ShouldUpdateCoords()
    {
        var element = LabelElement.CreateText(0, 0, "A", 10);
        element.UpdatePosition(50.5m, 60.5m);
        
        Assert.Equal(50.5m, element.XMm);
        Assert.Equal(60.5m, element.YMm);
    }

    [Fact]
    public void UpdateContent_ShouldUpdateString()
    {
        var element = LabelElement.CreateText(0, 0, "Old", 10);
        element.UpdateContent("New");
        
        Assert.Equal("New", element.Content);
    }

    [Fact]
    public void UpdateStyle_ShouldUpdateVisualProps()
    {
        var element = LabelElement.CreateBarcode(0, 0, "C", 10, 1);
        element.UpdateStyle(40, 20m, 5);
        
        Assert.Equal(40, element.FontSize); // Even if barcode, property exists
        Assert.Equal(20m, element.HeightMm);
        Assert.Equal(5, element.BarWidth);
    }
}
