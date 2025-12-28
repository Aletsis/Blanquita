namespace Blanquita.Application.DTOs;

public record SupervisorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
}

public record CreateSupervisorDto
{
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateSupervisorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
}
