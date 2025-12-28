namespace Blanquita.Application.DTOs;

public record CashCollectionDto
{
    public int Id { get; init; }
    public int Thousands { get; init; }
    public int FiveHundreds { get; init; }
    public int TwoHundreds { get; init; }
    public int Hundreds { get; init; }
    public int Fifties { get; init; }
    public int Twenties { get; init; }
    public decimal TotalAmount { get; init; }
    public string CashRegisterName { get; init; } = string.Empty;
    public string CashierName { get; init; } = string.Empty;
    public string SupervisorName { get; init; } = string.Empty;
    public DateTime CollectionDateTime { get; init; }
    public int Folio { get; init; }
    public bool IsForCashCut { get; init; }
}

public record CreateCashCollectionDto
{
    public int Thousands { get; init; }
    public int FiveHundreds { get; init; }
    public int TwoHundreds { get; init; }
    public int Hundreds { get; init; }
    public int Fifties { get; init; }
    public int Twenties { get; init; }
    public string CashRegisterName { get; init; } = string.Empty;
    public string CashierName { get; init; } = string.Empty;
    public string SupervisorName { get; init; } = string.Empty;
    public bool IsForCashCut { get; init; }
}
