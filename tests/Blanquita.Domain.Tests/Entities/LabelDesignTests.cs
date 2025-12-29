using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class LabelDesignTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateLabelDesign()
    {
        // Arrange
        var name = "Standard Label";
        var width = 50.0m;
        var height = 20.0m;
        var marginTop = 1.0m;
        var marginLeft = 1.0m;
        var orientation = "N";

        // Act
        var design = LabelDesign.Create(name, width, height, marginTop, marginLeft, orientation);

        // Assert
        Assert.NotNull(design);
        Assert.Equal(name, design.Name);
        Assert.Equal(width, design.WidthInMm);
        Assert.Equal(height, design.HeightInMm);
        Assert.Equal(marginTop, design.MarginTopInMm);
        Assert.Equal(marginLeft, design.MarginLeftInMm);
        Assert.Equal(orientation, design.Orientation);
        Assert.True(design.IsActive);
        Assert.False(design.IsDefault);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => LabelDesign.Create(""));
    }

    [Fact]
    public void Create_InvalidDimensions_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => LabelDesign.Create("Valid Name", widthInMm: 0));
        Assert.Throws<ArgumentException>(() => LabelDesign.Create("Valid Name", heightInMm: -1));
    }

    [Fact]
    public void Create_InvalidOrientation_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => LabelDesign.Create("Valid Name", orientation: "X"));
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateProperties()
    {
        // Arrange
        var design = LabelDesign.Create("Old Name");
        var newName = "New Name";
        
        // Act
        design.Update(newName, 60m, 30m, 2m, 2m, "R", 40, 30, 50, 10m, 2);

        // Assert
        Assert.Equal(newName, design.Name);
        Assert.Equal(60m, design.WidthInMm);
        Assert.Equal("R", design.Orientation);
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultTrue()
    {
        var design = LabelDesign.Create("Design");
        design.SetAsDefault();
        Assert.True(design.IsDefault);
    }

    [Fact]
    public void RemoveDefault_ShouldSetIsDefaultFalse()
    {
        var design = LabelDesign.Create("Design", isDefault: true);
        design.RemoveDefault();
        Assert.False(design.IsDefault);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var design = LabelDesign.Create("Design");
        design.Deactivate();
        Assert.False(design.IsActive);
    }

    [Fact]
    public void AddElement_ShouldAddElementToList()
    {
        var design = LabelDesign.Create("Design");
        var element = LabelElement.CreateText(10, 10, "{Name}", 20);

        design.AddElement(element);

        Assert.Contains(element, design.Elements);
    }

    [Fact]
    public void RemoveElement_ShouldRemoveElementFromList()
    {
        var design = LabelDesign.Create("Design");
        var element = LabelElement.CreateText(10, 10, "{Name}", 20);
        design.AddElement(element);

        design.RemoveElement(element);

        Assert.DoesNotContain(element, design.Elements);
    }

    [Fact]
    public void ClearElements_ShouldRemoveAllElements()
    {
        var design = LabelDesign.Create("Design");
        design.AddElement(LabelElement.CreateText(10, 10, "{Name}", 20));
        design.AddElement(LabelElement.CreateText(10, 20, "{Price}", 20));

        design.ClearElements();

        Assert.Empty(design.Elements);
    }
}
