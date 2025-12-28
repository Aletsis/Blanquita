namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO base para solicitudes de búsqueda por rango de fechas.
/// Encapsula la lógica de normalización de fechas para consultas.
/// </summary>
public class DateRangeSearchRequest
{
    /// <summary>
    /// Fecha de inicio del rango (opcional)
    /// </summary>
    public DateTime? FechaInicio { get; init; }

    /// <summary>
    /// Fecha de fin del rango (opcional)
    /// </summary>
    public DateTime? FechaFin { get; init; }

    /// <summary>
    /// Normaliza el rango de fechas para la búsqueda.
    /// Si no se especifica fecha de inicio, usa DateTime.MinValue.
    /// Si no se especifica fecha de fin, usa DateTime.MaxValue.
    /// Si la fecha fin no tiene hora, incluye todo el día (hasta las 23:59:59.9999999).
    /// </summary>
    public (DateTime Inicio, DateTime Fin) GetNormalizedDateRange()
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
    /// Verifica si se especificó fecha de inicio
    /// </summary>
    public bool HasFechaInicio() => FechaInicio.HasValue;

    /// <summary>
    /// Verifica si se especificó fecha de fin
    /// </summary>
    public bool HasFechaFin() => FechaFin.HasValue;

    /// <summary>
    /// Verifica si se especificó algún filtro de fecha
    /// </summary>
    public bool HasDateFilter() => HasFechaInicio() || HasFechaFin();

    /// <summary>
    /// Valida que el rango de fechas sea correcto
    /// </summary>
    public void Validate()
    {
        if (FechaInicio.HasValue && FechaFin.HasValue && FechaInicio > FechaFin)
        {
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha de fin");
        }

        if (FechaInicio.HasValue && FechaInicio > DateTime.Now)
        {
            throw new ArgumentException("La fecha de inicio no puede ser futura");
        }
    }

    /// <summary>
    /// Obtiene la cantidad de días en el rango
    /// </summary>
    public int GetDaysInRange()
    {
        if (!HasFechaInicio() || !HasFechaFin())
            return 0;

        return (FechaFin!.Value - FechaInicio!.Value).Days;
    }
}
