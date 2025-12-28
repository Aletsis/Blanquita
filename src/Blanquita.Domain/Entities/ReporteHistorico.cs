using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un reporte histórico de ventas.
/// Encapsula la lógica de negocio relacionada con reportes de corte de caja.
/// </summary>
public class ReporteHistorico : BaseEntity
{
    public Sucursal Sucursal { get; private set; }
    public DateTime Fecha { get; private set; }
    public decimal TotalSistema { get; private set; }
    public decimal TotalCorteManual { get; private set; }
    public string Notas { get; private set; }
    public DateTime FechaGeneracion { get; private set; }

    private readonly List<DetalleReporte> _detalles = new();
    public IReadOnlyList<DetalleReporte> Detalles => _detalles.AsReadOnly();

    /// <summary>
    /// Propiedad calculada que representa la diferencia entre el corte manual y el sistema.
    /// Un valor positivo indica superávit, negativo indica déficit.
    /// </summary>
    public decimal Diferencia => TotalCorteManual - TotalSistema;

    private ReporteHistorico() 
    { 
        Notas = string.Empty;
    } // Para EF Core

    private ReporteHistorico(
        Sucursal sucursal,
        DateTime fecha,
        decimal totalSistema,
        decimal totalCorteManual,
        List<DetalleReporte> detalles)
    {
        Sucursal = sucursal ?? throw new ArgumentNullException(nameof(sucursal));
        Fecha = fecha;
        TotalSistema = totalSistema;
        TotalCorteManual = totalCorteManual;
        FechaGeneracion = DateTime.Now;
        Notas = string.Empty;
        _detalles = detalles ?? new List<DetalleReporte>();
    }

    /// <summary>
    /// Crea una nueva instancia de ReporteHistorico con validaciones de negocio.
    /// </summary>
    public static ReporteHistorico Crear(
        Sucursal sucursal,
        DateTime fecha,
        decimal totalSistema,
        decimal totalCorteManual,
        List<DetalleReporte> detalles)
    {
        if (sucursal == null)
            throw new ArgumentNullException(nameof(sucursal));

        if (totalSistema < 0)
            throw new ArgumentException("El total del sistema no puede ser negativo", nameof(totalSistema));

        if (totalCorteManual < 0)
            throw new ArgumentException("El total del corte manual no puede ser negativo", nameof(totalCorteManual));

        if (fecha > DateTime.Now)
            throw new ArgumentException("La fecha del reporte no puede ser futura", nameof(fecha));

        return new ReporteHistorico(sucursal, fecha, totalSistema, totalCorteManual, detalles);
    }

    /// <summary>
    /// Actualiza las notas del reporte.
    /// </summary>
    public void ActualizarNotas(string notas)
    {
        if (notas == null)
            throw new ArgumentNullException(nameof(notas));

        Notas = notas;
    }

    /// <summary>
    /// Verifica si el reporte tiene alguna diferencia entre el sistema y el corte manual.
    /// </summary>
    public bool TieneDiferencia() => Diferencia != 0;

    /// <summary>
    /// Verifica si el reporte tiene un superávit (más dinero en el corte manual que en el sistema).
    /// </summary>
    public bool TieneSuperavit() => Diferencia > 0;

    /// <summary>
    /// Verifica si el reporte tiene un déficit (menos dinero en el corte manual que en el sistema).
    /// </summary>
    public bool TieneDeficit() => Diferencia < 0;

    /// <summary>
    /// Obtiene el porcentaje de diferencia respecto al total del sistema.
    /// </summary>
    public decimal ObtenerPorcentajeDiferencia()
    {
        if (TotalSistema == 0)
            return 0;

        return (Diferencia / TotalSistema) * 100;
    }

    /// <summary>
    /// Verifica si el reporte tiene detalles.
    /// </summary>
    public bool TieneDetalles() => _detalles.Any();

    /// <summary>
    /// Obtiene el número de detalles en el reporte.
    /// </summary>
    public int ObtenerCantidadDetalles() => _detalles.Count;

    /// <summary>
    /// Agrega un detalle al reporte.
    /// </summary>
    public void AgregarDetalle(DetalleReporte detalle)
    {
        if (detalle == null)
            throw new ArgumentNullException(nameof(detalle));

        _detalles.Add(detalle);
    }
}
