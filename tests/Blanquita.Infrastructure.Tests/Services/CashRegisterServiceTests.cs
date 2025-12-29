using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Services;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class CashRegisterServiceTests
{
    private readonly Mock<ICashRegisterRepository> _repositoryMock;
    private readonly CashRegisterService _service;

    public CashRegisterServiceTests()
    {
        _repositoryMock = new Mock<ICashRegisterRepository>();
        _service = new CashRegisterService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        var entity = CashRegister.Create("Caja 1", "192.168.1.1", 9100, 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, 5);

        _repositoryMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Caja 1", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_IfNameExists()
    {
        var dto = new CreateCashRegisterDto { Name = "Duplicate", PrinterIp = "1.1.1.1", PrinterPort = 9100, BranchId = 1 };

        _repositoryMock.Setup(x => x.ExistsAsync("Duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldUnsetExistingLastRegister_WhenNewIsLast()
    {
        var dto = new CreateCashRegisterDto 
        { 
            Name = "New", 
            PrinterIp = "1.1.1.1", 
            PrinterPort = 9100, 
            BranchId = 1, 
            IsLastRegister = true 
        };

        var existingLast = CashRegister.Create("Old Last", "2.2.2.2", 9100, 1, true);

        _repositoryMock.Setup(x => x.ExistsAsync("New", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(x => x.GetLastRegisterByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLast);

        await _service.CreateAsync(dto);

        // Verify that the existing last register was unset
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<CashRegister>(r => !r.IsLastRegister), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<CashRegister>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_IfNotFound()
    {
        var dto = new UpdateCashRegisterDto { Id = 999, Name = "Test", PrinterIp = "1.1.1.1", PrinterPort = 9100, BranchId = 1 };

        _repositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CashRegister?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(dto));
    }

    [Fact]
    public async Task GetByBranchAsync_ShouldFilterByBranch()
    {
        var r1 = CashRegister.Create("R1", "1.1.1.1", 9100, 1);
        var r2 = CashRegister.Create("R2", "2.2.2.2", 9100, 1);
        var registers = new List<CashRegister> { r1, r2 };

        _repositoryMock.Setup(x => x.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registers);

        var result = await _service.GetByBranchAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBackupRegisterAsync_ShouldReturnNextOrPrevious()
    {
        var current = CashRegister.Create("Current", "1.1.1.1", 9100, 1, false);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(current, 5);

        var backup = CashRegister.Create("Backup", "2.2.2.2", 9100, 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(backup, 6);

        _repositoryMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        _repositoryMock.Setup(x => x.GetByIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(backup);

        var result = await _service.GetBackupRegisterAsync(5);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
    }
}
