using Blanquita.Domain.ValueObjects;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que encapsula los parámetros de búsqueda de reportes históricos.
/// Incluye lógica de normalización de fechas para facilitar las consultas.
/// </summary>
public sealed class BuscarReportesRequest
{
    public Sucursal? Sucursal { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }

    /// <summary>
    /// Normaliza el rango de fechas para la búsqueda.
    /// Si no se especifica fecha de inicio, usa DateTime.MinValue.
    /// Si no se especifica fecha de fin, usa DateTime.MaxValue.
    /// Si la fecha fin no tiene hora, incluye todo el día (hasta las 23:59:59.9999999).
    /// </summary>
    public (DateTime inicio, DateTime fin) GetNormalizedDateRange()
    {
        var inicio = FechaInicio ?? DateTime.MinValue;
        var fin = FechaFin ?? DateTime.MaxValue;

        // Si la fecha fin no tiene hora (es medianoche), incluir todo el día
        if (fin != DateTime.MaxValue && fin.TimeOfDay == TimeSpan.Zero)
        {
            fin = fin.AddDays(1).AddTicks(-1);
        }

        return (inicio, fin);
    }

    /// <summary>
    /// Verifica si se especificó algún filtro de sucursal.
    /// </summary>
    public bool TieneFiltroSucursal() => Sucursal != null;

    /// <summary>
    /// Verifica si se especificó algún filtro de fecha.
    /// </summary>
    public bool TieneFiltroFecha() => FechaInicio.HasValue || FechaFin.HasValue;

    /// <summary>
    /// Verifica si se especificó algún filtro.
    /// </summary>
    public bool TieneFiltros() => TieneFiltroSucursal() || TieneFiltroFecha();
}
