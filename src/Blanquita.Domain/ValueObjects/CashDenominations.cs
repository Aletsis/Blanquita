namespace Blanquita.Domain.ValueObjects;

public record CashDenominations
{
    public int Thousands { get; }
    public int FiveHundreds { get; }
    public int TwoHundreds { get; }
    public int Hundreds { get; }
    public int Fifties { get; }
    public int Twenties { get; }

    // Constructor requerido por EF Core
    private CashDenominations() { }

    private CashDenominations(int thousands, int fiveHundreds, int twoHundreds, 
        int hundreds, int fifties, int twenties)
    {
        Thousands = thousands;
        FiveHundreds = fiveHundreds;
        TwoHundreds = twoHundreds;
        Hundreds = hundreds;
        Fifties = fifties;
        Twenties = twenties;
    }

    public static CashDenominations Create(int thousands, int fiveHundreds, int twoHundreds,
        int hundreds, int fifties, int twenties)
    {
        if (thousands < 0 || fiveHundreds < 0 || twoHundreds < 0 || 
            hundreds < 0 || fifties < 0 || twenties < 0)
            throw new ArgumentException("Denominations cannot be negative");

        return new CashDenominations(thousands, fiveHundreds, twoHundreds, 
            hundreds, fifties, twenties);
    }

    public static CashDenominations Zero => new(0, 0, 0, 0, 0, 0);

    public Money CalculateTotal()
    {
        var total = (Thousands * 1000m) +
                   (FiveHundreds * 500m) +
                   (TwoHundreds * 200m) +
                   (Hundreds * 100m) +
                   (Fifties * 50m) +
                   (Twenties * 20m);

        return Money.Create(total);
    }
}
