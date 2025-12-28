namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa el diseño de una etiqueta para impresión.
/// Contiene las dimensiones y configuración de layout para etiquetas Zebra.
/// </summary>
public class LabelDesignDto
{
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
    /// N = Normal, R = Rotado 90°
    /// </summary>
    public string Orientation { get; set; } = "N";
}
