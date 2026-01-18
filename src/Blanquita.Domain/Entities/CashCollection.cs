using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a cash collection (money pickup) during the day
/// </summary>
public class CashCollection : BaseEntity
{
    public CashDenominations Denominations { get; private set; }
    public string CashRegisterName { get; private set; }
    public string CashierName { get; private set; }
    public string SupervisorName { get; private set; }
    public DateTime CollectionDateTime { get; private set; }
    public int Folio { get; private set; }
    public bool IsForCashCut { get; private set; }

    // EF Core constructor
    private CashCollection() { }

    private CashCollection(CashDenominations denominations, string cashRegisterName,
        string cashierName, string supervisorName, DateTime collectionDateTime,
        int folio, bool isForCashCut)
    {
        Denominations = denominations;
        CashRegisterName = cashRegisterName;
        CashierName = cashierName;
        SupervisorName = supervisorName;
        CollectionDateTime = collectionDateTime;
        Folio = folio;
        IsForCashCut = isForCashCut;
    }

    public static CashCollection Create(
        int thousands, int fiveHundreds, int twoHundreds,
        int hundreds, int fifties, int twenties,
        string cashRegisterName, string cashierName, string supervisorName,
        int folio, bool isForCashCut = false)
    {
        if (string.IsNullOrWhiteSpace(cashRegisterName))
            throw new ArgumentException("Cash register name cannot be empty", nameof(cashRegisterName));

        if (string.IsNullOrWhiteSpace(cashierName))
            throw new ArgumentException("Cashier name cannot be empty", nameof(cashierName));

        if (string.IsNullOrWhiteSpace(supervisorName))
            throw new ArgumentException("Supervisor name cannot be empty", nameof(supervisorName));

        if (folio <= 0)
            throw new ArgumentException("Folio must be greater than zero", nameof(folio));

        var denominations = CashDenominations.Create(thousands, fiveHundreds, twoHundreds,
            hundreds, fifties, twenties);

        return new CashCollection(denominations, cashRegisterName, cashierName,
            supervisorName, DateTime.UtcNow, folio, isForCashCut);
    }

    public Money GetTotalAmount() => Denominations.CalculateTotal();

    public void MarkAsForCashCut()
    {
        IsForCashCut = true;
    }
}
