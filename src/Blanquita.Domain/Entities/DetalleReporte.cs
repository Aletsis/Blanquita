namespace Blanquita.Domain.Entities;

/// <summary>
/// Entidad que representa una línea de detalle dentro de un reporte histórico.
/// Contiene la información de ventas por caja en una fecha específica.
/// </summary>
public class DetalleReporte : BaseEntity
{
    public string Fecha { get; private set; }
    public string Caja { get; private set; }
    public decimal Facturado { get; private set; }
    public decimal Devolucion { get; private set; }
    public decimal VentaGlobal { get; private set; }
    public decimal Total { get; private set; }

    // Navigation property
    public int ReporteHistoricoId { get; private set; }

    private DetalleReporte() { } // Para EF Core

    private DetalleReporte(
        string fecha,
        string caja,
        decimal facturado,
        decimal devolucion,
        decimal ventaGlobal,
        decimal total)
    {
        Fecha = fecha ?? throw new ArgumentNullException(nameof(fecha));
        Caja = caja ?? throw new ArgumentNullException(nameof(caja));
        Facturado = facturado;
        Devolucion = devolucion;
        VentaGlobal = ventaGlobal;
        Total = total;
    }

    public static DetalleReporte Crear(
        string fecha,
        string caja,
        decimal facturado,
        decimal devolucion,
        decimal ventaGlobal,
        decimal total)
    {
        if (string.IsNullOrWhiteSpace(fecha))
            throw new ArgumentException("La fecha no puede estar vacía", nameof(fecha));

        if (string.IsNullOrWhiteSpace(caja))
            throw new ArgumentException("La caja no puede estar vacía", nameof(caja));

        return new DetalleReporte(fecha, caja, facturado, devolucion, ventaGlobal, total);
    }

    /// <summary>
    /// Calcula el total neto considerando facturado, devoluciones y venta global.
    /// </summary>
    public decimal CalcularTotalNeto() => Facturado - Devolucion + VentaGlobal;

    /// <summary>
    /// Verifica si este detalle tiene devoluciones.
    /// </summary>
    public bool TieneDevoluciones() => Devolucion > 0;

    /// <summary>
    /// Verifica si este detalle tiene venta global.
    /// </summary>
    public bool TieneVentaGlobal() => VentaGlobal > 0;
}
