namespace Blanquita.Domain.ValueObjects;

public record CashCutTotals
{
    public int TotalThousands { get; }
    public int TotalFiveHundreds { get; }
    public int TotalTwoHundreds { get; }
    public int TotalHundreds { get; }
    public int TotalFifties { get; }
    public int TotalTwenties { get; }
    public Money TotalSlips { get; }
    public Money TotalCards { get; }

    // Constructor requerido por EF Core
    private CashCutTotals() { }

    private CashCutTotals(int totalThousands, int totalFiveHundreds, int totalTwoHundreds,
        int totalHundreds, int totalFifties, int totalTwenties,
        Money totalSlips, Money totalCards)
    {
        TotalThousands = totalThousands;
        TotalFiveHundreds = totalFiveHundreds;
        TotalTwoHundreds = totalTwoHundreds;
        TotalHundreds = totalHundreds;
        TotalFifties = totalFifties;
        TotalTwenties = totalTwenties;
        TotalSlips = totalSlips;
        TotalCards = totalCards;
    }

    public static CashCutTotals Create(int totalThousands, int totalFiveHundreds, int totalTwoHundreds,
        int totalHundreds, int totalFifties, int totalTwenties,
        decimal totalSlips, decimal totalCards)
    {
        if (totalThousands < 0 || totalFiveHundreds < 0 || totalTwoHundreds < 0 ||
            totalHundreds < 0 || totalFifties < 0 || totalTwenties < 0)
            throw new ArgumentException("Totals cannot be negative");

        return new CashCutTotals(
            totalThousands, totalFiveHundreds, totalTwoHundreds,
            totalHundreds, totalFifties, totalTwenties,
            Money.Create(totalSlips), Money.Create(totalCards));
    }

    /// <summary>
    /// Calcula el total de las recolecciones (suma de todas las denominaciones)
    /// </summary>
    public Money CalculateCollectionsTotal()
    {
        var cashTotal = (TotalThousands * 1000m) +
                       (TotalFiveHundreds * 500m) +
                       (TotalTwoHundreds * 200m) +
                       (TotalHundreds * 100m) +
                       (TotalFifties * 50m) +
                       (TotalTwenties * 20m);

        return Money.Create(cashTotal);
    }

    /// <summary>
    /// Calcula el efectivo a entregar: Total Tira - Total Recolecciones - Total Tarjetas
    /// </summary>
    public Money CalculateCashToDeliver()
    {
        var collectionsTotal = CalculateCollectionsTotal();
        var cashToDeliver = TotalSlips.Amount - collectionsTotal.Amount - TotalCards.Amount;
        return Money.Create(cashToDeliver);
    }

    /// <summary>
    /// DEPRECATED: Use CalculateCollectionsTotal() instead
    /// Mantenido por compatibilidad temporal
    /// </summary>
    [Obsolete("Use CalculateCollectionsTotal() instead")]
    public Money CalculateGrandTotal()
    {
        return CalculateCollectionsTotal();
    }
}
