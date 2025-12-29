namespace Blanquita.Domain.Entities;

/// <summary>
/// Entidad que representa la configuración de diseño de etiquetas para impresoras Zebra.
/// Almacena dimensiones, fuentes, márgenes y otros parámetros de diseño.
/// </summary>
public class LabelDesign : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo de la configuración de diseño.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Ancho de la etiqueta en puntos (dots). 
    /// Valor estándar: 607 (2 pulgadas a 300dpi)
    /// </summary>
    public int WidthInDots { get; private set; } = 607;
    
    /// <summary>
    /// Alto de la etiqueta en puntos (dots).
    /// Valor estándar: 199
    /// </summary>
    public int HeightInDots { get; private set; } = 199;
    
    /// <summary>
    /// Margen superior en puntos.
    /// </summary>
    public int MarginTop { get; private set; } = 20;
    
    /// <summary>
    /// Margen izquierdo en puntos.
    /// </summary>
    public int MarginLeft { get; private set; } = 20;
    
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
    /// Altura del código de barras en puntos.
    /// </summary>
    public int BarcodeHeight { get; private set; } = 100;

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
        int widthInDots = 607,
        int heightInDots = 199,
        int marginTop = 20,
        int marginLeft = 20,
        string orientation = "N",
        int productNameFontSize = 50,
        int productCodeFontSize = 40,
        int priceFontSize = 60,
        int barcodeHeight = 100,
        int barcodeWidth = 3,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la configuración no puede estar vacío.", nameof(name));

        if (widthInDots <= 0)
            throw new ArgumentException("El ancho debe ser mayor a 0.", nameof(widthInDots));

        if (heightInDots <= 0)
            throw new ArgumentException("El alto debe ser mayor a 0.", nameof(heightInDots));

        if (!new[] { "N", "R", "I", "B" }.Contains(orientation))
            throw new ArgumentException("Orientación inválida. Use N, R, I o B.", nameof(orientation));

        if (barcodeWidth < 1 || barcodeWidth > 10)
            throw new ArgumentException("El ancho del código de barras debe estar entre 1 y 10.", nameof(barcodeWidth));

        return new LabelDesign
        {
            Name = name,
            WidthInDots = widthInDots,
            HeightInDots = heightInDots,
            MarginTop = marginTop,
            MarginLeft = marginLeft,
            Orientation = orientation,
            ProductNameFontSize = productNameFontSize,
            ProductCodeFontSize = productCodeFontSize,
            PriceFontSize = priceFontSize,
            BarcodeHeight = barcodeHeight,
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
        int widthInDots,
        int heightInDots,
        int marginTop,
        int marginLeft,
        string orientation,
        int productNameFontSize,
        int productCodeFontSize,
        int priceFontSize,
        int barcodeHeight,
        int barcodeWidth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la configuración no puede estar vacío.", nameof(name));

        if (widthInDots <= 0)
            throw new ArgumentException("El ancho debe ser mayor a 0.", nameof(widthInDots));

        if (heightInDots <= 0)
            throw new ArgumentException("El alto debe ser mayor a 0.", nameof(heightInDots));

        if (!new[] { "N", "R", "I", "B" }.Contains(orientation))
            throw new ArgumentException("Orientación inválida. Use N, R, I o B.", nameof(orientation));

        if (barcodeWidth < 1 || barcodeWidth > 10)
            throw new ArgumentException("El ancho del código de barras debe estar entre 1 y 10.", nameof(barcodeWidth));

        Name = name;
        WidthInDots = widthInDots;
        HeightInDots = heightInDots;
        MarginTop = marginTop;
        MarginLeft = marginLeft;
        Orientation = orientation;
        ProductNameFontSize = productNameFontSize;
        ProductCodeFontSize = productCodeFontSize;
        PriceFontSize = priceFontSize;
        BarcodeHeight = barcodeHeight;
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
}
