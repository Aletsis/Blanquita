using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class DelivererMapper
{
    public static DelivererDto ToDto(this Deliverer entity)
    {
        return new DelivererDto
        {
            Id = entity.Id,
            EmployeeNumber = entity.EmployeeNumber,
            Name = entity.Name,
            BranchId = entity.BranchId.Value,
            IsActive = entity.IsActive
        };
    }

    public static Deliverer ToEntity(this CreateDelivererDto dto)
    {
        return Deliverer.Create(
            dto.EmployeeNumber,
            dto.Name,
            dto.BranchId,
            dto.IsActive);
    }

    public static void UpdateEntity(this UpdateDelivererDto dto, Deliverer entity)
    {
        entity.UpdateName(dto.Name);
        entity.UpdateEmployeeNumber(dto.EmployeeNumber);
        entity.UpdateBranch(dto.BranchId);
        
        if (dto.IsActive) entity.Activate();
        else entity.Deactivate();
    }
}
