using System;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Representa un elemento individual dentro del diseño de una etiqueta (Ej. Texto, Código de Barras).
/// </summary>
public class LabelElement : BaseEntity
{
    /// <summary>
    /// Identificador del diseño al que pertenece este elemento.
    /// </summary>
    public int LabelDesignId { get; private set; }
    
    public LabelDesign LabelDesign { get; private set; } = null!;

    /// <summary>
    /// Tipo de elemento: "Text", "Barcode".
    /// </summary>
    public string ElementType { get; private set; } = "Text";

    /// <summary>
    /// Posición X en milímetros (desde el margen izquierdo).
    /// </summary>
    public decimal XMm { get; private set; }

    /// <summary>
    /// Posición Y en milímetros (desde el margen superior).
    /// </summary>
    public decimal YMm { get; private set; }

    /// <summary>
    /// Contenido o variable a imprimir.
    /// Ejemplos: "{ProductName}", "{Price}", "{ProductCode}", "OFERTA", "{Date}"
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Tamaño de la fuente (altura en puntos) para texto.
    /// </summary>
    public int FontSize { get; private set; } = 30;

    /// <summary>
    /// Altura en milímetros (para código de barras).
    /// Si es null, usa el valor por defecto del diseño o uno estándar.
    /// </summary>
    public decimal? HeightMm { get; private set; }

    /// <summary>
    /// Ancho de barra (1-10) para código de barras.
    /// </summary>
    public int? BarWidth { get; private set; }

    /// <summary>
    /// Constructor privado para EF Core.
    /// </summary>
    private LabelElement() { }

    public static LabelElement Create(
        string elementType,
        decimal xMm,
        decimal yMm,
        string content,
        int fontSize,
        decimal? heightMm,
        int? barWidth)
    {
        return new LabelElement
        {
            ElementType = elementType,
            XMm = xMm,
            YMm = yMm,
            Content = content,
            FontSize = fontSize,
            HeightMm = heightMm,
            BarWidth = barWidth
        };
    }

    public static LabelElement CreateText(decimal x, decimal y, string content, int fontSize)
    {
        return new LabelElement
        {
            ElementType = "Text",
            XMm = x,
            YMm = y,
            Content = content,
            FontSize = fontSize
        };
    }

    public static LabelElement CreateBarcode(decimal x, decimal y, string contentVariable, decimal heightMm, int barWidth = 2)
    {
        return new LabelElement
        {
            ElementType = "Barcode",
            XMm = x,
            YMm = y,
            Content = contentVariable,
            HeightMm = heightMm,
            BarWidth = barWidth
        };
    }
    
    public void UpdatePosition(decimal x, decimal y)
    {
        XMm = x;
        YMm = y;
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }
    
    public void UpdateStyle(int fontSize, decimal? heightMm, int? barWidth)
    {
        FontSize = fontSize;
        HeightMm = heightMm;
        BarWidth = barWidth;
    }
}
