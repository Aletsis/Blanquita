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
            IdContpaqi = cashRegister.IdContpaqi,
            PrinterIp = cashRegister.PrinterConfig.IpAddress,
            PrinterPort = cashRegister.PrinterConfig.Port,
            BranchId = cashRegister.BranchId.Value,
            Tipo = cashRegister.Tipo,
            IsLastRegister = cashRegister.IsLastRegister
        };
    }

    public static CashRegister ToEntity(this CreateCashRegisterDto dto)
    {
        return CashRegister.Create(dto.Name, dto.Serie, dto.IdContpaqi, dto.PrinterIp, dto.PrinterPort, 
            dto.BranchId, dto.IsLastRegister, dto.Tipo);
    }

    public static void UpdateEntity(this UpdateCashRegisterDto dto, CashRegister cashRegister)
    {
        cashRegister.UpdateName(dto.Name);
        cashRegister.UpdateSerie(dto.Serie);
        cashRegister.UpdateIdContpaqi(dto.IdContpaqi);
        cashRegister.UpdatePrinterConfiguration(dto.PrinterIp, dto.PrinterPort);
        cashRegister.UpdateBranch(dto.BranchId);
        cashRegister.UpdateTipo(dto.Tipo);

        if (dto.IsLastRegister)
            cashRegister.SetAsLastRegister();
        else
            cashRegister.UnsetAsLastRegister();
    }
}

