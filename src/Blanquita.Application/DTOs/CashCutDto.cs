namespace Blanquita.Application.DTOs;

public record CashCutDto
{
    public int Id { get; init; }
    public int TotalThousands { get; init; }
    public int TotalFiveHundreds { get; init; }
    public int TotalTwoHundreds { get; init; }
    public int TotalHundreds { get; init; }
    public int TotalFifties { get; init; }
    public int TotalTwenties { get; init; }
    public decimal TotalSlips { get; init; }
    public decimal TotalCards { get; init; }
    
    /// <summary>
    /// Total de las recolecciones (suma de denominaciones)
    /// </summary>
    public decimal CollectionsTotal { get; init; }
    
    /// <summary>
    /// Efectivo a entregar = Total Tira - Total Tarjetas - Total Recolecciones
    /// </summary>
    public decimal CashToDeliver { get; init; }
    
    /// <summary>
    /// DEPRECATED: Use CollectionsTotal instead
    /// </summary>
    [Obsolete("Use CollectionsTotal instead")]
    public decimal GrandTotal { get; init; }
    
    public string CashRegisterName { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public string SupervisorName { get; init; } = string.Empty;
    public string CashierName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public DateTime CutDateTime { get; init; }

    // FoxPro specific fields
    public int CashRegisterId { get; init; }
    public string RawInvoices { get; init; } = string.Empty; // From CFACTURA
    public string RawReturns { get; init; } = string.Empty;  // From CDEVOLUCIO
}

public record CreateCashCutDto
{
    public int TotalThousands { get; init; }
    public int TotalFiveHundreds { get; init; }
    public int TotalTwoHundreds { get; init; }
    public int TotalHundreds { get; init; }
    public int TotalFifties { get; init; }
    public int TotalTwenties { get; init; }
    public decimal TotalSlips { get; init; }
    public decimal TotalCards { get; init; }
    public string CashRegisterName { get; init; } = string.Empty;
    public string SupervisorName { get; init; } = string.Empty;
    public string CashierName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
}

public record ProcessCashCutRequest
{
    public int SupervisorId { get; init; }
    public int CashierId { get; init; }
    public int CashRegisterId { get; init; }
    public decimal TotalSlips { get; init; }
    public decimal TotalCards { get; init; }
}
