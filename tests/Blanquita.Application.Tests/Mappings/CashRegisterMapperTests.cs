using Blanquita.Application.DTOs;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Application.Tests.Mappings;

public class CashRegisterMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var entity = CashRegister.Create("Caja 1", "127.0.0.1", 9100, 1, true);
        var dto = entity.ToDto();

        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.PrinterConfig.IpAddress, dto.PrinterIp);
        Assert.Equal(entity.BranchId.Value, dto.BranchId);
        Assert.Equal(entity.IsLastRegister, dto.IsLastRegister);
    }

    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        var dto = new CreateCashRegisterDto
        {
            Name = "Caja 2",
            PrinterIp = "10.0.0.1",
            PrinterPort = 8080,
            BranchId = 2,
            IsLastRegister = false
        };

        var entity = CashRegisterMapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.PrinterIp, entity.PrinterConfig.IpAddress);
        Assert.Equal(dto.BranchId, entity.BranchId.Value);
    }

    [Fact]
    public void UpdateEntity_ShouldUpdateCorrectly()
    {
        var entity = CashRegister.Create("Old", "127.0.0.1", 9100, 1, false);
        var dto = new UpdateCashRegisterDto
        {
            Name = "New",
            PrinterIp = "1.1.1.1",
            PrinterPort = 9999,
            BranchId = 5,
            IsLastRegister = true
        };

        dto.UpdateEntity(entity);

        Assert.Equal("New", entity.Name);
        Assert.Equal("1.1.1.1", entity.PrinterConfig.IpAddress);
        Assert.Equal(9999, entity.PrinterConfig.Port);
        Assert.Equal(5, entity.BranchId.Value);
        Assert.True(entity.IsLastRegister);
    }
}
