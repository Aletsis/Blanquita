namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa una fila de datos en un reporte de corte de caja.
/// Contiene los totales por caja para un día específico.
/// </summary>
public class ReportRowDto
{
    public string Fecha { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public decimal Facturado { get; set; }
    public decimal Devolucion { get; set; }
    public decimal VentaGlobal { get; set; }
    public decimal Total { get; set; }
}
