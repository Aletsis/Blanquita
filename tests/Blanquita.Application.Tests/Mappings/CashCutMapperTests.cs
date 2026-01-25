using Blanquita.Application.DTOs;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Application.Tests.Mappings;

public class CashCutMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var entity = CashCut.Create(1, 1, 1, 1, 1, 1, 5000m, 30m, 20m, "Reg", "Sup", "Cash", "Branch");
        
        var dto = entity.ToDto();
        
        Assert.Equal(entity.Totals.TotalThousands, dto.TotalThousands);
        Assert.Equal(entity.Totals.TotalSlips.Amount, dto.TotalSlips);
        Assert.Equal(entity.Totals.TotalBanbajio.Amount, dto.TotalBanbajio);
        Assert.Equal(entity.Totals.TotalBanregio.Amount, dto.TotalBanregio);
        Assert.Equal(entity.Totals.TotalBanbajio.Amount + entity.Totals.TotalBanregio.Amount, dto.TotalCards);
        Assert.Equal(entity.CashRegisterName, dto.CashRegisterName);
        Assert.Equal(entity.SupervisorName, dto.SupervisorName);
        Assert.Equal(entity.CashierName, dto.CashierName);
        Assert.Equal(entity.BranchName, dto.BranchName);
        
        // Calculated fields verify
        Assert.Equal(entity.Totals.CalculateCollectionsTotal().Amount, dto.CollectionsTotal);
        Assert.Equal(entity.Totals.CalculateCashToDeliver().Amount, dto.CashToDeliver);
    }

    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        var dto = new CreateCashCutDto
        {
            TotalThousands = 1,
            TotalFiveHundreds = 2,
            TotalTwoHundreds = 3,
            TotalHundreds = 4,
            TotalFifties = 5,
            TotalTwenties = 6,
            TotalSlips = 1000m,
            TotalBanbajio = 300m,
            TotalBanregio = 200m,
            CashRegisterName = "Reg",
            SupervisorName = "Sup",
            CashierName = "Cash",
            BranchName = "Branch"
        };
        
        var entity = CashCutMapper.ToEntity(dto);
        
        Assert.Equal(dto.TotalThousands, entity.Totals.TotalThousands);
        Assert.Equal(dto.TotalSlips, entity.Totals.TotalSlips.Amount);
        Assert.Equal(dto.TotalBanbajio, entity.Totals.TotalBanbajio.Amount);
        Assert.Equal(dto.TotalBanregio, entity.Totals.TotalBanregio.Amount);
        Assert.Equal(dto.CashRegisterName, entity.CashRegisterName);
        Assert.Equal(dto.SupervisorName, entity.SupervisorName);
        Assert.Equal(dto.BranchName, entity.BranchName);
    }
}
