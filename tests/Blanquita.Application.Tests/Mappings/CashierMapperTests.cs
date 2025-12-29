using Blanquita.Application.DTOs;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Application.Tests.Mappings;

public class CashierMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var entity = Cashier.Create(123, "Juan", 1);
        var dto = entity.ToDto();
        
        Assert.Equal(entity.EmployeeNumber, dto.EmployeeNumber);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.BranchId.Value, dto.BranchId);
    }

    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        var dto = new CreateCashierDto { EmployeeNumber = 999, Name = "Pedro", BranchId = 2, IsActive = true };
        var entity = CashierMapper.ToEntity(dto);
        
        Assert.Equal(dto.EmployeeNumber, entity.EmployeeNumber);
        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void UpdateEntity_ShouldUpdateCorrectly()
    {
        var entity = Cashier.Create(1, "Old", 1);
        var dto = new UpdateCashierDto { EmployeeNumber = 2, Name = "New", BranchId = 3, IsActive = false };
        
        dto.UpdateEntity(entity);
        
        Assert.Equal(2, entity.EmployeeNumber);
        Assert.Equal("New", entity.Name);
        Assert.False(entity.IsActive);
    }
}
