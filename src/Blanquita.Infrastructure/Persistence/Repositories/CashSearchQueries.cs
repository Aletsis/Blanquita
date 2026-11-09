using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;

namespace Blanquita.Infrastructure.Persistence.Repositories;

/// <summary>
/// Arma las consultas de búsqueda de recolecciones y cortes para que EF las traduzca a SQL,
/// en lugar de cargar la tabla completa y filtrar en memoria.
/// </summary>
internal static class CashSearchQueries
{
    public static IQueryable<CashCollection> Apply(IQueryable<CashCollection> query, CashCollectionSearchCriteria criteria)
    {
        if (criteria.FromUtc.HasValue)
            query = query.Where(c => c.CollectionDateTime >= criteria.FromUtc.Value);

        if (criteria.ToUtc.HasValue)
            query = query.Where(c => c.CollectionDateTime <= criteria.ToUtc.Value);

        if (!string.IsNullOrWhiteSpace(criteria.CashRegisterName))
        {
            // Sin distinguir mayúsculas, igual que la búsqueda anterior en memoria
            var register = criteria.CashRegisterName.ToUpper();
            query = query.Where(c => c.CashRegisterName.ToUpper() == register);
        }

        if (criteria.IsForCashCut.HasValue)
            query = query.Where(c => c.IsForCashCut == criteria.IsForCashCut.Value);

        query = query.OrderByDescending(c => c.CollectionDateTime);

        return Page(query, criteria.Skip, criteria.Take);
    }

    public static IQueryable<CashCut> Apply(IQueryable<CashCut> query, CashCutSearchCriteria criteria)
    {
        if (criteria.FromUtc.HasValue)
            query = query.Where(c => c.CutDateTime >= criteria.FromUtc.Value);

        if (criteria.ToUtc.HasValue)
            query = query.Where(c => c.CutDateTime <= criteria.ToUtc.Value);

        if (!string.IsNullOrWhiteSpace(criteria.BranchName))
            query = query.Where(c => c.BranchName == criteria.BranchName);

        if (!string.IsNullOrWhiteSpace(criteria.CashRegisterName))
            query = query.Where(c => c.CashRegisterName == criteria.CashRegisterName);

        if (!string.IsNullOrWhiteSpace(criteria.CashierName))
        {
            var cashier = criteria.CashierName.ToUpper();
            query = query.Where(c => c.CashierName.ToUpper().Contains(cashier));
        }

        if (!string.IsNullOrWhiteSpace(criteria.SupervisorName))
        {
            var supervisor = criteria.SupervisorName.ToUpper();
            query = query.Where(c => c.SupervisorName.ToUpper().Contains(supervisor));
        }

        // Gran total con la misma fórmula que CashCutTotals.CalculateGrandTotal, escrita para que EF la traduzca
        if (criteria.MinGrandTotal.HasValue)
            query = query.Where(c =>
                c.Totals.TotalThousands * 1000m + c.Totals.TotalFiveHundreds * 500m + c.Totals.TotalTwoHundreds * 200m +
                c.Totals.TotalHundreds * 100m + c.Totals.TotalFifties * 50m + c.Totals.TotalTwenties * 20m
                >= criteria.MinGrandTotal.Value);

        if (criteria.MaxGrandTotal.HasValue)
            query = query.Where(c =>
                c.Totals.TotalThousands * 1000m + c.Totals.TotalFiveHundreds * 500m + c.Totals.TotalTwoHundreds * 200m +
                c.Totals.TotalHundreds * 100m + c.Totals.TotalFifties * 50m + c.Totals.TotalTwenties * 20m
                <= criteria.MaxGrandTotal.Value);

        return Page(Sort(query, criteria), criteria.Skip, criteria.Take);
    }

    private static IOrderedQueryable<CashCut> Sort(IQueryable<CashCut> query, CashCutSearchCriteria criteria)
    {
        var asc = criteria.SortAscending;

        return criteria.SortColumn?.ToLowerInvariant() switch
        {
            "cutdatetime" or "date" => asc ? query.OrderBy(c => c.CutDateTime) : query.OrderByDescending(c => c.CutDateTime),
            "cashregistername" or "register" => asc ? query.OrderBy(c => c.CashRegisterName) : query.OrderByDescending(c => c.CashRegisterName),
            "cashiername" or "cashier" => asc ? query.OrderBy(c => c.CashierName) : query.OrderByDescending(c => c.CashierName),
            "supervisorname" or "supervisor" => asc ? query.OrderBy(c => c.SupervisorName) : query.OrderByDescending(c => c.SupervisorName),
            "branchname" or "branch" => asc ? query.OrderBy(c => c.BranchName) : query.OrderByDescending(c => c.BranchName),
            "grandtotal" or "total" => asc
                ? query.OrderBy(c => c.Totals.TotalThousands * 1000m + c.Totals.TotalFiveHundreds * 500m + c.Totals.TotalTwoHundreds * 200m +
                                     c.Totals.TotalHundreds * 100m + c.Totals.TotalFifties * 50m + c.Totals.TotalTwenties * 20m)
                : query.OrderByDescending(c => c.Totals.TotalThousands * 1000m + c.Totals.TotalFiveHundreds * 500m + c.Totals.TotalTwoHundreds * 200m +
                                               c.Totals.TotalHundreds * 100m + c.Totals.TotalFifties * 50m + c.Totals.TotalTwenties * 20m),
            _ => query.OrderByDescending(c => c.CutDateTime)
        };
    }

    private static IQueryable<T> Page<T>(IQueryable<T> query, int skip, int? take)
    {
        if (skip > 0)
            query = query.Skip(skip);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query;
    }
}
