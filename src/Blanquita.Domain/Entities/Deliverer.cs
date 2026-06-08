using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a deliverer employee who handles order delivery routes
/// </summary>
public class Deliverer : BaseEntity
{
    public int EmployeeNumber { get; private set; }
    public string Name { get; private set; }
    public BranchId BranchId { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core constructor
    private Deliverer() { }

    private Deliverer(int employeeNumber, string name, BranchId branchId, bool isActive = true)
    {
        EmployeeNumber = employeeNumber;
        Name = name;
        BranchId = branchId;
        IsActive = isActive;
    }

    public static Deliverer Create(int employeeNumber, string name, int branchId, bool isActive = true)
    {
        if (employeeNumber <= 0)
            throw new ArgumentException("Employee number must be greater than zero", nameof(employeeNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new Deliverer(employeeNumber, name, BranchId.Create(branchId), isActive);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }

    public void UpdateEmployeeNumber(int employeeNumber)
    {
        if (employeeNumber <= 0)
            throw new ArgumentException("Employee number must be greater than zero", nameof(employeeNumber));

        EmployeeNumber = employeeNumber;
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
