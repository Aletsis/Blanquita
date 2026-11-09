using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Persistence;

public class CashSearchQueriesTests
{
    private static T WithDate<T>(T entity, string property, DateTime utc)
    {
        typeof(T).GetProperty(property)!.SetValue(entity, utc);
        return entity;
    }

    private static CashCollection Collection(string register, DateTime utc, bool isCut = false, int folio = 1)
    {
        var c = CashCollection.Create(1, 0, 0, 0, 0, 0, register, "Cajera", "Sup", folio, isCut);
        return WithDate(c, nameof(CashCollection.CollectionDateTime), utc);
    }

    private static CashCut Cut(string register, string cashier, int thousands, DateTime utc, string branch = "Centro")
    {
        var c = CashCut.Create(thousands, 0, 0, 0, 0, 0, 100m, 0m, 0m, register, "Supervisor", cashier, branch);
        return WithDate(c, nameof(CashCut.CutDateTime), utc);
    }

    private static readonly DateTime Day = new(2026, 11, 3, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Collections_ShouldFilterByRegisterIgnoringCase_DateRangeAndCutStatus()
    {
        var data = new[]
        {
            Collection("CAJA 1", Day),
            Collection("caja 1", Day.AddHours(1), isCut: true),
            Collection("CAJA 2", Day),
            Collection("CAJA 1", Day.AddDays(-2))
        }.AsQueryable();

        var result = CashSearchQueries.Apply(data, new CashCollectionSearchCriteria
        {
            FromUtc = Day.AddHours(-6),
            ToUtc = Day.AddHours(6),
            CashRegisterName = "Caja 1",
            IsForCashCut = false
        }).ToList();

        Assert.Single(result);
        Assert.Equal(Day, result[0].CollectionDateTime);
    }

    [Fact]
    public void Collections_ShouldOrderNewestFirstAndPage()
    {
        var data = Enumerable.Range(1, 5)
            .Select(i => Collection("CAJA 1", Day.AddMinutes(i), folio: i))
            .AsQueryable();

        var result = CashSearchQueries.Apply(data, new CashCollectionSearchCriteria { Skip = 1, Take = 2 }).ToList();

        Assert.Equal(new[] { 4, 3 }, result.Select(c => c.Folio));
    }

    [Fact]
    public void Cuts_ShouldFilterByCashierSubstringAndGrandTotalRange()
    {
        var data = new[]
        {
            Cut("CAJA 1", "Maria Lopez", thousands: 2, Day),   // 2000
            Cut("CAJA 1", "MARIANA", thousands: 5, Day),      // 5000
            Cut("CAJA 2", "Juana", thousands: 3, Day)         // 3000
        }.AsQueryable();

        var result = CashSearchQueries.Apply(data, new CashCutSearchCriteria
        {
            CashierName = "maria",
            MinGrandTotal = 1000m,
            MaxGrandTotal = 4000m
        }).ToList();

        Assert.Single(result);
        Assert.Equal("Maria Lopez", result[0].CashierName);
    }

    [Fact]
    public void Cuts_ShouldSortByGrandTotal_AndFilterByBranch()
    {
        var data = new[]
        {
            Cut("CAJA 1", "A", thousands: 2, Day),
            Cut("CAJA 2", "B", thousands: 5, Day),
            Cut("CAJA 3", "C", thousands: 3, Day),
            Cut("CAJA 4", "D", thousands: 9, Day, branch: "Norte")
        }.AsQueryable();

        var result = CashSearchQueries.Apply(data, new CashCutSearchCriteria
        {
            BranchName = "Centro",
            SortColumn = "GrandTotal",
            SortAscending = false
        }).ToList();

        Assert.Equal(new[] { "B", "C", "A" }, result.Select(c => c.CashierName));
    }

    [Fact]
    public void Cuts_ShouldDefaultToNewestFirst()
    {
        var data = new[]
        {
            Cut("CAJA 1", "Vieja", 1, Day.AddHours(-1)),
            Cut("CAJA 1", "Nueva", 1, Day)
        }.AsQueryable();

        var result = CashSearchQueries.Apply(data, new CashCutSearchCriteria()).ToList();

        Assert.Equal("Nueva", result[0].CashierName);
    }

    // ToQueryString genera el SQL sin conectarse: falla si EF no puede traducir algún filtro u orden
    private static BlanquitaDbContext NpgsqlContext() =>
        new(new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseNpgsql("Host=localhost;Database=no_se_conecta")
            .Options);

    [Fact]
    public void Collections_AllFilters_TranslateToSql()
    {
        using var context = NpgsqlContext();

        var sql = CashSearchQueries.Apply(context.CashCollections, new CashCollectionSearchCriteria
        {
            FromUtc = Day, ToUtc = Day.AddDays(1), CashRegisterName = "Caja 1", IsForCashCut = false, Skip = 10, Take = 10
        }).ToQueryString();

        Assert.Contains("upper(", sql);
        Assert.Contains("LIMIT", sql);
    }

    [Theory]
    [InlineData("grandtotal")]
    [InlineData("cashier")]
    [InlineData(null)]
    public void Cuts_AllFilters_TranslateToSql(string? sortColumn)
    {
        using var context = NpgsqlContext();

        var sql = CashSearchQueries.Apply(context.CashCuts, new CashCutSearchCriteria
        {
            FromUtc = Day, ToUtc = Day.AddDays(1), BranchName = "Centro", CashRegisterName = "Caja 1",
            CashierName = "maria", SupervisorName = "juan", MinGrandTotal = 100m, MaxGrandTotal = 5000m,
            SortColumn = sortColumn, SortAscending = false, Skip = 0, Take = 20
        }).ToQueryString();

        Assert.Contains("ORDER BY", sql);
        Assert.Contains("LIMIT", sql);
    }
}
