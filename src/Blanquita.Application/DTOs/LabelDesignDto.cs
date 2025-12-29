namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa el diseño de una etiqueta para impresión.
/// Contiene las dimensiones y configuración de layout para etiquetas Zebra.
/// </summary>
public class LabelDesignDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre descriptivo de la configuración de diseño.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ancho de la etiqueta en puntos (dots). 
    /// Valor estándar: 607 (2 pulgadas a 300dpi)
    /// </summary>
    public int WidthInDots { get; set; } = 607;
    
    /// <summary>
    /// Alto de la etiqueta en puntos (dots).
    /// Valor estándar: 199
    /// </summary>
    public int HeightInDots { get; set; } = 199;
    
    /// <summary>
    /// Margen superior en puntos.
    /// </summary>
    public int MarginTop { get; set; } = 20;
    
    /// <summary>
    /// Margen izquierdo en puntos.
    /// </summary>
    public int MarginLeft { get; set; } = 20;
    
    /// <summary>
    /// Orientación de la etiqueta.
    /// N = Normal, R = Rotado 90°, I = Invertido, B = Bottom-up
    /// </summary>
    public string Orientation { get; set; } = "N";

    /// <summary>
    /// Tamaño de fuente para el nombre del producto.
    /// </summary>
    public int ProductNameFontSize { get; set; } = 50;

    /// <summary>
    /// Tamaño de fuente para el código del producto.
    /// </summary>
    public int ProductCodeFontSize { get; set; } = 40;

    /// <summary>
    /// Tamaño de fuente para el precio.
    /// </summary>
    public int PriceFontSize { get; set; } = 60;

    /// <summary>
    /// Altura del código de barras en puntos.
    /// </summary>
    public int BarcodeHeight { get; set; } = 100;

    /// <summary>
    /// Ancho de las barras del código de barras (1-10).
    /// </summary>
    public int BarcodeWidth { get; set; } = 3;

    /// <summary>
    /// Indica si esta es la configuración predeterminada.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indica si la configuración está activa.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
