using Blanquita.Application.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace Blanquita.Application.Helpers;

/// <summary>
/// Centraliza todos los cálculos financieros y aritméticos de recolección y corte de caja
/// para respetar la separación de responsabilidades y evitar lógica incrustada en la UI.
/// </summary>
public static class FinancialCalculator
{
    /// <summary>
    /// Suma el total en efectivo de un listado de recolecciones basándose en sus denominaciones.
    /// </summary>
    public static decimal CalculateCollectionsTotal(IEnumerable<CashCollectionDto> collections)
    {
        if (collections == null) return 0m;
        
        return collections.Sum(c => 
            (c.Thousands * 1000m) + 
            (c.FiveHundreds * 500m) + 
            (c.TwoHundreds * 200m) + 
            (c.Hundreds * 100m) + 
            (c.Fifties * 50m) + 
            (c.Twenties * 20m));
    }

    /// <summary>
    /// Calcula el monto total a partir de las cantidades de billetes por denominación.
    /// </summary>
    public static decimal CalculateDenominationsTotal(int thousands, int fiveHundreds, int twoHundreds, int hundreds, int fifties, int twenties)
    {
        return (thousands * 1000m) + 
               (fiveHundreds * 500m) + 
               (twoHundreds * 200m) + 
               (hundreds * 100m) + 
               (fifties * 50m) + 
               (twenties * 20m);
    }

    /// <summary>
    /// Calcula el efectivo a entregar restando tarjetas y recolecciones del total reportado en la tira de efectivo.
    /// </summary>
    public static decimal CalculateCashToDeliver(decimal totalSlips, decimal totalBanbajio, decimal totalBanregio, decimal totalCollections)
    {
        var totalCards = totalBanbajio + totalBanregio;
        return totalSlips - totalCards - totalCollections;
    }
}
