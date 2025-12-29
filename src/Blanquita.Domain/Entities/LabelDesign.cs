using System.Collections.Generic;
using System.Linq;
using System;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Entidad que representa la configuración de diseño de etiquetas para impresoras Zebra.
/// Almacena dimensiones en milímetros, fuentes, márgenes y otros parámetros de diseño.
/// </summary>
public class LabelDesign : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo de la configuración de diseño.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Elementos configurables de la etiqueta (Texto, Códigos de barras, etc).
    /// </summary>
    private readonly List<LabelElement> _elements = new();
    public IReadOnlyCollection<LabelElement> Elements => _elements.AsReadOnly();

    /// <summary>
    /// Ancho de la etiqueta en milímetros. 
    /// Valor estándar: 51.6 mm (aproximadamente 2 pulgadas)
    /// </summary>
    public decimal WidthInMm { get; private set; } = 51.6m;
    
    /// <summary>
    /// Alto de la etiqueta en milímetros.
    /// Valor estándar: 16.9 mm
    /// </summary>
    public decimal HeightInMm { get; private set; } = 16.9m;
    
    /// <summary>
    /// Margen superior en milímetros.
    /// </summary>
    public decimal MarginTopInMm { get; private set; } = 1.7m;
    
    /// <summary>
    /// Margen izquierdo en milímetros.
    /// </summary>
    public decimal MarginLeftInMm { get; private set; } = 1.7m;
    
    /// <summary>
    /// Orientación de la etiqueta.
    /// N = Normal, R = Rotado 90°, I = Invertido, B = Bottom-up
    /// </summary>
    public string Orientation { get; private set; } = "N";

    /// <summary>
    /// Tamaño de fuente para el nombre del producto.
    /// </summary>
    public int ProductNameFontSize { get; private set; } = 50;

    /// <summary>
    /// Tamaño de fuente para el código del producto.
    /// </summary>
    public int ProductCodeFontSize { get; private set; } = 40;

    /// <summary>
    /// Tamaño de fuente para el precio.
    /// </summary>
    public int PriceFontSize { get; private set; } = 60;

    /// <summary>
    /// Altura del código de barras en milímetros.
    /// </summary>
    public decimal BarcodeHeightInMm { get; private set; } = 8.5m;

    /// <summary>
    /// Ancho de las barras del código de barras (1-10).
    /// </summary>
    public int BarcodeWidth { get; private set; } = 3;

    /// <summary>
    /// Indica si esta es la configuración predeterminada.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Indica si la configuración está activa.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // Constructor privado para EF Core
    private LabelDesign() { }

    /// <summary>
    /// Crea una nueva configuración de diseño de etiqueta.
    /// </summary>
    public static LabelDesign Create(
        string name,
        decimal widthInMm = 51.6m,
        decimal heightInMm = 16.9m,
        decimal marginTopInMm = 1.7m,
        decimal marginLeftInMm = 1.7m,
        string orientation = "N",
        int productNameFontSize = 50,
        int productCodeFontSize = 40,
        int priceFontSize = 60,
        decimal barcodeHeightInMm = 8.5m,
        int barcodeWidth = 3,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la configuración no puede estar vacío.", nameof(name));

        if (widthInMm <= 0)
            throw new ArgumentException("El ancho debe ser mayor a 0.", nameof(widthInMm));

        if (heightInMm <= 0)
            throw new ArgumentException("El alto debe ser mayor a 0.", nameof(heightInMm));

        if (!new[] { "N", "R", "I", "B" }.Contains(orientation))
            throw new ArgumentException("Orientación inválida. Use N, R, I o B.", nameof(orientation));

        if (barcodeWidth < 1 || barcodeWidth > 10)
            throw new ArgumentException("El ancho del código de barras debe estar entre 1 y 10.", nameof(barcodeWidth));

        return new LabelDesign
        {
            Name = name,
            WidthInMm = widthInMm,
            HeightInMm = heightInMm,
            MarginTopInMm = marginTopInMm,
            MarginLeftInMm = marginLeftInMm,
            Orientation = orientation,
            ProductNameFontSize = productNameFontSize,
            ProductCodeFontSize = productCodeFontSize,
            PriceFontSize = priceFontSize,
            BarcodeHeightInMm = barcodeHeightInMm,
            BarcodeWidth = barcodeWidth,
            IsDefault = isDefault,
            IsActive = true
        };
    }

    /// <summary>
    /// Actualiza la configuración de diseño.
    /// </summary>
    public void Update(
        string name,
        decimal widthInMm,
        decimal heightInMm,
        decimal marginTopInMm,
        decimal marginLeftInMm,
        string orientation,
        int productNameFontSize,
        int productCodeFontSize,
        int priceFontSize,
        decimal barcodeHeightInMm,
        int barcodeWidth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la configuración no puede estar vacío.", nameof(name));

        if (widthInMm <= 0)
            throw new ArgumentException("El ancho debe ser mayor a 0.", nameof(widthInMm));

        if (heightInMm <= 0)
            throw new ArgumentException("El alto debe ser mayor a 0.", nameof(heightInMm));

        if (!new[] { "N", "R", "I", "B" }.Contains(orientation))
            throw new ArgumentException("Orientación inválida. Use N, R, I o B.", nameof(orientation));

        if (barcodeWidth < 1 || barcodeWidth > 10)
            throw new ArgumentException("El ancho del código de barras debe estar entre 1 y 10.", nameof(barcodeWidth));

        Name = name;
        WidthInMm = widthInMm;
        HeightInMm = heightInMm;
        MarginTopInMm = marginTopInMm;
        MarginLeftInMm = marginLeftInMm;
        Orientation = orientation;
        ProductNameFontSize = productNameFontSize;
        ProductCodeFontSize = productCodeFontSize;
        PriceFontSize = priceFontSize;
        BarcodeHeightInMm = barcodeHeightInMm;
        BarcodeWidth = barcodeWidth;
    }

    /// <summary>
    /// Establece esta configuración como predeterminada.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
    }

    /// <summary>
    /// Remueve el estado de predeterminada.
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Activa la configuración.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Desactiva la configuración.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    public void AddElement(LabelElement element)
    {
        _elements.Add(element);
    }

    public void RemoveElement(LabelElement element)
    {
        _elements.Remove(element);
    }
    
    public void ClearElements()
    {
        _elements.Clear();
    }
}
