using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Application.Tests.Mappings;

public class CashCollectionMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var entity = CashCollection.Create(1, 2, 3, 4, 5, 6, "Register", "Cashier", "Sup", 123);
        
        var dto = entity.ToDto();

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Denominations.Thousands, dto.Thousands);
        Assert.Equal(entity.Denominations.FiveHundreds, dto.FiveHundreds);
        Assert.Equal(entity.GetTotalAmount().Amount, dto.TotalAmount);
        Assert.Equal(entity.CashRegisterName, dto.CashRegisterName);
        Assert.Equal(entity.CashierName, dto.CashierName);
        Assert.Equal(entity.SupervisorName, dto.SupervisorName);
        Assert.Equal(entity.Folio, dto.Folio);
        Assert.Equal(entity.CollectionDateTime, dto.CollectionDateTime);
    }
}
