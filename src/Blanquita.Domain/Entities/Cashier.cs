using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a cashier employee who operates cash registers
/// </summary>
public class Cashier : BaseEntity
{
    public int EmployeeNumber { get; private set; }
    public string Name { get; private set; }
    public BranchId BranchId { get; private set; }
    public bool IsActive { get; private set; }
    public int IDContpaq { get; private set; }

    // EF Core constructor
    private Cashier() { }

    private Cashier(int employeeNumber, string name, BranchId branchId, int idContpaq = 0, bool isActive = true)
    {
        EmployeeNumber = employeeNumber;
        Name = name;
        BranchId = branchId;
        IDContpaq = idContpaq;
        IsActive = isActive;
    }

    public static Cashier Create(int employeeNumber, string name, int branchId, int idContpaq = 0, bool isActive = true)
    {
        if (employeeNumber <= 0)
            throw new ArgumentException("Employee number must be greater than zero", nameof(employeeNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new Cashier(employeeNumber, name, BranchId.Create(branchId), idContpaq, isActive);
    }

    public void UpdateIDContpaq(int idContpaq)
    {
        IDContpaq = idContpaq;
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
