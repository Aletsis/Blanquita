namespace Blanquita.Application.Constants;

public static class LabelVariables
{
    public const string ProductName = "{ProductName}";
    public const string ProductCode = "{ProductCode}";
    public const string Price = "{Price}";
    public const string PriceNoFormat = "{PriceNoFormat}";
    public const string Date = "{Date}";
    
    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { ProductName, "Nombre Producto" },
        { ProductCode, "Código Producto" },
        { Price, "Precio (con signo $)" },
        { PriceNoFormat, "Precio (sin formato)" },
        { Date, "Fecha Actual" }
    };
}
