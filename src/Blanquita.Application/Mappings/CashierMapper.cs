using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class CashierMapper
{
    public static CashierDto ToDto(this Cashier cashier)
    {
        return new CashierDto
        {
            Id = cashier.Id,
            EmployeeNumber = cashier.EmployeeNumber,
            Name = cashier.Name,
            BranchId = cashier.BranchId,
            IsActive = cashier.IsActive
        };
    }

    public static Cashier ToEntity(this CreateCashierDto dto)
    {
        return Cashier.Create(dto.EmployeeNumber, dto.Name, dto.BranchId, dto.IsActive);
    }

    public static void UpdateEntity(this UpdateCashierDto dto, Cashier cashier)
    {
        cashier.UpdateName(dto.Name);
        cashier.UpdateEmployeeNumber(dto.EmployeeNumber);
        cashier.UpdateBranch(dto.BranchId);

        if (dto.IsActive)
            cashier.Activate();
        else
            cashier.Deactivate();
    }
}
