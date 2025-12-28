namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa un reporte histórico de corte de caja completo.
/// Utilizado para la persistencia en archivos JSON.
/// </summary>
public class ReporteDto
{
    public int Id { get; set; }
    public string Sucursal { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal TotalCorteManual { get; set; }
    public decimal Diferencia { get; set; }
    public string Notas { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public List<ReportRowDto> Detalles { get; set; } = new();
}
