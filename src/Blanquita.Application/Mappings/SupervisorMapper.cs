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
            EmployeeNumber = supervisor.EmployeeNumber,
            Name = supervisor.Name,
            BranchId = supervisor.BranchId,
            PhoneNumber = supervisor.PhoneNumber,
            IsActive = supervisor.IsActive
        };
    }

    public static Supervisor ToEntity(this CreateSupervisorDto dto)
    {
        return Supervisor.Create(dto.EmployeeNumber, dto.Name, dto.BranchId, dto.PhoneNumber, dto.IsActive);
    }

    public static void UpdateEntity(this UpdateSupervisorDto dto, Supervisor supervisor)
    {
        supervisor.UpdateEmployeeNumber(dto.EmployeeNumber);
        supervisor.UpdateName(dto.Name);
        supervisor.UpdateBranch(dto.BranchId);
        supervisor.UpdatePhoneNumber(dto.PhoneNumber);

        if (dto.IsActive)
            supervisor.Activate();
        else
            supervisor.Deactivate();
    }
}
