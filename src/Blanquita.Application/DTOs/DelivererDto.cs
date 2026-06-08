namespace Blanquita.Application.DTOs;

public class DelivererDto
{
    public int Id { get; set; }
    public int EmployeeNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDelivererDto
{
    public int EmployeeNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDelivererDto
{
    public int Id { get; set; }
    public int EmployeeNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public bool IsActive { get; set; }
}

public class SearchDelivererRequest : PagedSearchRequest
{
    public int? BranchId { get; set; }
    public bool? IsActive { get; set; }
    public int? EmployeeNumber { get; set; }

    public bool HasBranchFilter() => BranchId.HasValue && BranchId.Value > 0;
    public bool HasActiveFilter() => IsActive.HasValue;
    public bool HasEmployeeNumberFilter() => EmployeeNumber.HasValue;
}
