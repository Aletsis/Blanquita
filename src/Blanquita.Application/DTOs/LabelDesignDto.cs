using System.Collections.Generic;
using System;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa el diseño de una etiqueta para impresión.
/// Contiene las dimensiones y configuración de layout para etiquetas Zebra.
/// Las dimensiones se especifican en milímetros para facilitar la configuración.
/// </summary>
public class LabelDesignDto
{
    /// <summary>
    /// DPI estándar para impresoras Zebra (300 DPI).
    /// </summary>
    public const int StandardDPI = 300;
    
    /// <summary>
    /// Factor de conversión: 1 pulgada = 25.4 mm
    /// </summary>
    private const double MmPerInch = 25.4;
    
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre descriptivo de la configuración de diseño.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public List<LabelElementDto> Elements { get; set; } = new();

    /// <summary>
    /// Ancho de la etiqueta en milímetros. 
    /// Valor estándar: 51.6 mm (aproximadamente 2 pulgadas)
    /// </summary>
    public decimal WidthInMm { get; set; } = 51.6m;
    
    /// <summary>
    /// Alto de la etiqueta en milímetros.
    /// Valor estándar: 16.9 mm
    /// </summary>
    public decimal HeightInMm { get; set; } = 16.9m;
    
    /// <summary>
    /// Margen superior en milímetros.
    /// </summary>
    public decimal MarginTopInMm { get; set; } = 1.7m;
    
    /// <summary>
    /// Margen izquierdo en milímetros.
    /// </summary>
    public decimal MarginLeftInMm { get; set; } = 1.7m;
    
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
    /// Altura del código de barras en milímetros.
    /// </summary>
    public decimal BarcodeHeightInMm { get; set; } = 8.5m;

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
    
    /// <summary>
    /// Convierte milímetros a puntos (dots) usando el DPI estándar.
    /// </summary>
    public static int MmToDots(decimal mm, int dpi = StandardDPI)
    {
        return (int)Math.Round(mm / (decimal)MmPerInch * dpi);
    }
    
    /// <summary>
    /// Convierte puntos (dots) a milímetros usando el DPI estándar.
    /// </summary>
    public static decimal DotsToMm(int dots, int dpi = StandardDPI)
    {
        return Math.Round((decimal)dots * (decimal)MmPerInch / dpi, 2);
    }
    
    /// <summary>
    /// Obtiene el ancho en puntos para la impresora.
    /// </summary>
    public int GetWidthInDots() => MmToDots(WidthInMm);
    
    /// <summary>
    /// Obtiene el alto en puntos para la impresora.
    /// </summary>
    public int GetHeightInDots() => MmToDots(HeightInMm);
    
    /// <summary>
    /// Obtiene el margen superior en puntos para la impresora.
    /// </summary>
    public int GetMarginTopInDots() => MmToDots(MarginTopInMm);
    
    /// <summary>
    /// Obtiene el margen izquierdo en puntos para la impresora.
    /// </summary>
    public int GetMarginLeftInDots() => MmToDots(MarginLeftInMm);
    
    /// <summary>
    /// Obtiene la altura del código de barras en puntos para la impresora.
    /// </summary>
    public int GetBarcodeHeightInDots() => MmToDots(BarcodeHeightInMm);
}
