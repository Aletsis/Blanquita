using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a supervisor who manages cashiers and cash operations
/// </summary>
public class Supervisor : BaseEntity
{
    public string Name { get; private set; }
    public BranchId BranchId { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core constructor
    private Supervisor() { }

    private Supervisor(string name, BranchId branchId, bool isActive = true)
    {
        Name = name;
        BranchId = branchId;
        IsActive = isActive;
    }

    public static Supervisor Create(string name, int branchId, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new Supervisor(name, BranchId.Create(branchId), isActive);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }

    public void UpdateBranch(int branchId)
    {
        BranchId = BranchId.Create(branchId);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
