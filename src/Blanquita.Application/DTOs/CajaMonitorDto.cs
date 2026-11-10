using System;
using System.Collections.Generic;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO con la información de monitoreo de una caja registradora con turno/apertura.
/// </summary>
public record CajaMonitorDto
{
    public int CashRegisterId { get; init; }
    public string CashRegisterName { get; init; } = string.Empty;
    public int IdContpaqi { get; init; }
    public int BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public int ShiftId { get; init; }
    public int CashierId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public DateTime OpeningTime { get; init; }
    public DateTime? ClosingTime { get; init; }
    public int Status { get; init; } // 0 = Abierta, 1 = Cerrada
    public bool IsClosed => Status == 1;
    public bool IsConciliated { get; init; }
    public int? ConciliacionId { get; init; }

    // Formas de pago cobradas
    public decimal CashCollected { get; init; }
    public decimal CardCollected { get; init; }
    public decimal ReturnsTotal { get; init; }
    public decimal TotalSold => CashCollected + CardCollected;

    // Recolecciones realizadas
    public decimal TotalRecolectado { get; init; }
    public List<CashCollectionDto> Recolecciones { get; init; } = new();
    public int TotalRecoleccionesCount => Recolecciones.Count;

    // Salidas de efectivo (si están registradas)
    public decimal SalidasEfectivo { get; init; }

    // Efectivo estimado disponible en caja (Cobrado - Recolectado - Salidas)
    public decimal EfectivoEstimadoEnCaja => CashCollected - TotalRecolectado - SalidasEfectivo;
}
