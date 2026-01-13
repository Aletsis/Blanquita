using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Mappings;

public static class CashRegisterMapper
{
    public static CashRegisterDto ToDto(this CashRegister cashRegister)
    {
        return new CashRegisterDto
        {
            Id = cashRegister.Id,
            Name = cashRegister.Name,
            Serie = cashRegister.Serie,
            PrinterIp = cashRegister.PrinterConfig.IpAddress,
            PrinterPort = cashRegister.PrinterConfig.Port,
            BranchId = cashRegister.BranchId.Value,
            IsLastRegister = cashRegister.IsLastRegister
        };
    }

    public static CashRegister ToEntity(this CreateCashRegisterDto dto)
    {
        return CashRegister.Create(dto.Name, dto.Serie, dto.PrinterIp, dto.PrinterPort, 
            dto.BranchId, dto.IsLastRegister);
    }

    public static void UpdateEntity(this UpdateCashRegisterDto dto, CashRegister cashRegister)
    {
        cashRegister.UpdateName(dto.Name);
        cashRegister.UpdateSerie(dto.Serie);
        cashRegister.UpdatePrinterConfiguration(dto.PrinterIp, dto.PrinterPort);
        cashRegister.UpdateBranch(dto.BranchId);

        if (dto.IsLastRegister)
            cashRegister.SetAsLastRegister();
        else
            cashRegister.UnsetAsLastRegister();
    }
}
