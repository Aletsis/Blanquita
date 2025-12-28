namespace Blanquita.Application.DTOs;

public record CashRegisterDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PrinterIp { get; init; } = string.Empty;
    public int PrinterPort { get; init; }
    public int BranchId { get; init; }
    public bool IsLastRegister { get; init; }
}

public record CreateCashRegisterDto
{
    public string Name { get; init; } = string.Empty;
    public string PrinterIp { get; init; } = string.Empty;
    public int PrinterPort { get; init; } = 9100;
    public int BranchId { get; init; }
    public bool IsLastRegister { get; init; }
}

public record UpdateCashRegisterDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PrinterIp { get; init; } = string.Empty;
    public int PrinterPort { get; init; }
    public int BranchId { get; init; }
    public bool IsLastRegister { get; init; }
}
