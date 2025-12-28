namespace Blanquita.Application.DTOs;

public record CashierDto
{
    public int Id { get; init; }
    public int EmployeeNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
}

public record CreateCashierDto
{
    public int EmployeeNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateCashierDto
{
    public int Id { get; init; }
    public int EmployeeNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
}
