using MudBlazor;

namespace Blanquita.Web.Helpers;

/// <summary>
/// Helper estático que proporciona utilidades para la presentación de reportes en la UI.
/// Centraliza la lógica de colores, iconos y formateo relacionado con reportes.
/// </summary>
public static class ReporteUIHelper
{
    /// <summary>
    /// Obtiene el color apropiado para mostrar una diferencia en un reporte.
    /// - Verde (Success): Sin diferencia (0)
    /// - Azul (Info): Superávit (positivo)
    /// - Amarillo (Warning): Déficit (negativo)
    /// </summary>
    public static Color ObtenerColorDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => Color.Success,
            > 0 => Color.Info,
            < 0 => Color.Warning
        };
    }

    /// <summary>
    /// Obtiene el icono apropiado para mostrar una diferencia en un reporte.
    /// </summary>
    public static string ObtenerIconoDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => Icons.Material.Filled.CheckCircle,
            > 0 => Icons.Material.Filled.TrendingUp,
            < 0 => Icons.Material.Filled.TrendingDown
        };
    }

    /// <summary>
    /// Obtiene un mensaje descriptivo para una diferencia.
    /// </summary>
    public static string ObtenerMensajeDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => "Sin diferencia",
            > 0 => $"Superávit de {diferencia:C2}",
            < 0 => $"Déficit de {Math.Abs(diferencia):C2}"
        };
    }

    /// <summary>
    /// Obtiene la severidad apropiada para mostrar alertas relacionadas con diferencias.
    /// </summary>
    public static Severity ObtenerSeveridadDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => Severity.Success,
            > 0 => Severity.Info,
            < 0 => Severity.Warning
        };
    }

    /// <summary>
    /// Formatea una diferencia con signo y formato de moneda.
    /// </summary>
    public static string FormatearDiferencia(decimal diferencia)
    {
        var signo = diferencia switch
        {
            > 0 => "+",
            < 0 => "-",
            _ => ""
        };

        return $"{signo}{Math.Abs(diferencia):C2}";
    }
}
