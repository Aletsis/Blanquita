using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a supervisor who manages cashiers and cash operations
/// </summary>
public class Supervisor : BaseEntity
{
    public int EmployeeNumber { get; private set; }
    public string Name { get; private set; }
    public BranchId BranchId { get; private set; }
    public bool IsActive { get; private set; }
    public string? PhoneNumber { get; private set; }

    // EF Core constructor
    private Supervisor() { }

    private Supervisor(int employeeNumber, string name, BranchId branchId, string? phoneNumber, bool isActive = true)
    {
        EmployeeNumber = employeeNumber;
        Name = name;
        BranchId = branchId;
        PhoneNumber = phoneNumber;
        IsActive = isActive;
    }

    public static Supervisor Create(int employeeNumber, string name, int branchId, string? phoneNumber = null, bool isActive = true)
    {
        if (employeeNumber <= 0)
            throw new ArgumentException("Employee number must be greater than zero", nameof(employeeNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        string? validatedPhone = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            validatedPhone = ValueObjects.PhoneNumber.Create(phoneNumber).Value;
        }

        return new Supervisor(employeeNumber, name, BranchId.Create(branchId), validatedPhone, isActive);
    }

    public void UpdateEmployeeNumber(int employeeNumber)
    {
        if (employeeNumber <= 0)
            throw new ArgumentException("Employee number must be greater than zero", nameof(employeeNumber));

        EmployeeNumber = employeeNumber;
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

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = null;
        }
        else
        {
            PhoneNumber = ValueObjects.PhoneNumber.Create(phoneNumber).Value;
        }
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
