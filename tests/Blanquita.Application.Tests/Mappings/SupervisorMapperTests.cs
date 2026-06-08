using Blanquita.Application.DTOs;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Application.Tests.Mappings;

public class SupervisorMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var entity = Supervisor.Create(123, "Juan", 1, null, true);
        var dto = entity.ToDto();
        Assert.Equal(entity.EmployeeNumber, dto.EmployeeNumber);
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        var dto = new CreateSupervisorDto { EmployeeNumber = 123, Name = "Pedro", BranchId = 2, IsActive = true };
        var entity = SupervisorMapper.ToEntity(dto);
        Assert.Equal(dto.EmployeeNumber, entity.EmployeeNumber);
        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void UpdateEntity_ShouldUpdateCorrectly()
    {
        var entity = Supervisor.Create(123, "Old", 1);
        var dto = new UpdateSupervisorDto { EmployeeNumber = 456, Name = "New", BranchId = 2, IsActive = false };
        
        dto.UpdateEntity(entity);
        
        Assert.Equal(456, entity.EmployeeNumber);
        Assert.Equal("New", entity.Name);
        Assert.False(entity.IsActive);
    }
}
