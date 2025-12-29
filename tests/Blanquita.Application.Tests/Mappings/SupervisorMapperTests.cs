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
        var entity = Supervisor.Create("Juan", 1, true);
        var dto = entity.ToDto();
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        var dto = new CreateSupervisorDto { Name = "Pedro", BranchId = 2, IsActive = true };
        var entity = SupervisorMapper.ToEntity(dto);
        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void UpdateEntity_ShouldUpdateCorrectly()
    {
        var entity = Supervisor.Create("Old", 1);
        var dto = new UpdateSupervisorDto { Name = "New", BranchId = 2, IsActive = false };
        
        dto.UpdateEntity(entity);
        
        Assert.Equal("New", entity.Name);
        Assert.False(entity.IsActive);
    }
}
