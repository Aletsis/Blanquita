namespace Blanquita.Application.DTOs;

/// <summary>
/// Datos de una alerta de descuadre. Se serializa en la cola de Hangfire, por eso solo lleva valores simples.
/// </summary>
public class DescuadreAlertDto
{
    public int ShiftId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public decimal TotalEntregado { get; set; }
    public decimal TotalEsperado { get; set; }
    public decimal Diferencia { get; set; }
}
