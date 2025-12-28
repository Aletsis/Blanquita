using Blanquita.Domain.Enums;

namespace Blanquita.Application.DTOs;

/// <summary>
/// Request para búsqueda de reportes con filtros
/// </summary>
public class SearchReportRequest
{
    /// <summary>
    /// Tipo de reporte a buscar
    /// </summary>
    public TipoReporte TipoReporte { get; set; }
    
    /// <summary>
    /// Fecha de inicio del rango de búsqueda (opcional)
    /// </summary>
    public DateTime? FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha de fin del rango de búsqueda (opcional)
    /// </summary>
    public DateTime? FechaFin { get; set; }
    
    /// <summary>
    /// Normaliza las fechas para incluir el día completo
    /// </summary>
    public (DateTime Start, DateTime End) GetNormalizedDateRange()
    {
        var start = FechaInicio ?? DateTime.MinValue;
        var end = FechaFin ?? DateTime.MaxValue;
        
        // Si la fecha de fin no tiene hora específica, incluir todo el día
        if (end.TimeOfDay == TimeSpan.Zero)
        {
            end = end.AddDays(1).AddTicks(-1);
        }
        
        return (start, end);
    }
}
