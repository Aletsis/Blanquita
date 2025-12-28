using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class SupervisorMapper
{
    public static SupervisorDto ToDto(this Supervisor supervisor)
    {
        return new SupervisorDto
        {
            Id = supervisor.Id,
            Name = supervisor.Name,
            BranchId = supervisor.BranchId,
            IsActive = supervisor.IsActive
        };
    }

    public static Supervisor ToEntity(this CreateSupervisorDto dto)
    {
        return Supervisor.Create(dto.Name, dto.BranchId, dto.IsActive);
    }

    public static void UpdateEntity(this UpdateSupervisorDto dto, Supervisor supervisor)
    {
        supervisor.UpdateName(dto.Name);
        supervisor.UpdateBranch(dto.BranchId);

        if (dto.IsActive)
            supervisor.Activate();
        else
            supervisor.Deactivate();
    }
}
