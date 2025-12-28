using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a cash register (point of sale) with printer configuration
/// </summary>
public class CashRegister : BaseEntity
{
    public string Name { get; private set; }
    public PrinterConfiguration PrinterConfig { get; private set; }
    public BranchId BranchId { get; private set; }
    public bool IsLastRegister { get; private set; }

    // EF Core constructor
    private CashRegister() { }

    private CashRegister(string name, PrinterConfiguration printerConfig, BranchId branchId, bool isLastRegister = false)
    {
        Name = name;
        PrinterConfig = printerConfig;
        BranchId = branchId;
        IsLastRegister = isLastRegister;
    }

    public static CashRegister Create(string name, string printerIp, int printerPort, int branchId, bool isLastRegister = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var printerConfig = PrinterConfiguration.Create(printerIp, printerPort);
        var branch = BranchId.Create(branchId);

        return new CashRegister(name, printerConfig, branch, isLastRegister);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }

    public void UpdatePrinterConfiguration(string printerIp, int printerPort)
    {
        PrinterConfig = PrinterConfiguration.Create(printerIp, printerPort);
    }

    public void UpdateBranch(int branchId)
    {
        BranchId = BranchId.Create(branchId);
    }

    public void SetAsLastRegister()
    {
        IsLastRegister = true;
    }

    public void UnsetAsLastRegister()
    {
        IsLastRegister = false;
    }
}
