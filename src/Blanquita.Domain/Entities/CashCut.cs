using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a cash cut (end of shift cash count) aggregate root
/// </summary>
public class CashCut : BaseEntity
{
    public CashCutTotals Totals { get; private set; }
    public string CashRegisterName { get; private set; }
    public string SupervisorName { get; private set; }
    public string CashierName { get; private set; }
    public string BranchName { get; private set; }
    public DateTime CutDateTime { get; private set; }

    // EF Core constructor
    private CashCut() { }

    private CashCut(CashCutTotals totals, string cashRegisterName, string supervisorName,
        string cashierName, string branchName, DateTime cutDateTime)
    {
        Totals = totals;
        CashRegisterName = cashRegisterName;
        SupervisorName = supervisorName;
        CashierName = cashierName;
        BranchName = branchName;
        CutDateTime = cutDateTime;
    }

    public static CashCut Create(
        int totalThousands, int totalFiveHundreds, int totalTwoHundreds,
        int totalHundreds, int totalFifties, int totalTwenties,
        decimal totalSlips, decimal totalCards,
        string cashRegisterName, string supervisorName, string cashierName, string branchName)
    {
        if (string.IsNullOrWhiteSpace(cashRegisterName))
            throw new ArgumentException("Cash register name cannot be empty", nameof(cashRegisterName));

        if (string.IsNullOrWhiteSpace(supervisorName))
            throw new ArgumentException("Supervisor name cannot be empty", nameof(supervisorName));

        if (string.IsNullOrWhiteSpace(cashierName))
            throw new ArgumentException("Cashier name cannot be empty", nameof(cashierName));

        if (string.IsNullOrWhiteSpace(branchName))
            throw new ArgumentException("Branch name cannot be empty", nameof(branchName));

        var totals = CashCutTotals.Create(totalThousands, totalFiveHundreds, totalTwoHundreds,
            totalHundreds, totalFifties, totalTwenties, totalSlips, totalCards);

        return new CashCut(totals, cashRegisterName, supervisorName, cashierName,
            branchName, DateTime.UtcNow);
    }

    public Money GetGrandTotal() => Totals.CalculateGrandTotal();

    public bool IsValid()
    {
        // Business rule: A cash cut is valid if it has a grand total greater than zero
        return GetGrandTotal().Amount > 0;
    }
}
