using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Services;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class CashierServiceTests
{
    private readonly Mock<ICashierRepository> _repositoryMock;
    private readonly CashierService _service;

    public CashierServiceTests()
    {
        _repositoryMock = new Mock<ICashierRepository>();
        _service = new CashierService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        var entity = Cashier.Create(123, "Juan", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, 5);

        _repositoryMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Juan", result.Name);
        Assert.Equal(123, result.EmployeeNumber);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_IfEmployeeNumberExists()
    {
        var dto = new CreateCashierDto { EmployeeNumber = 123, Name = "Test", BranchId = 1 };

        _repositoryMock.Setup(x => x.ExistsAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepo_WhenValid()
    {
        var dto = new CreateCashierDto { EmployeeNumber = 456, Name = "Pedro", BranchId = 2, IsActive = true };

        _repositoryMock.Setup(x => x.ExistsAsync(456, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Cashier? capturedEntity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Cashier>(), It.IsAny<CancellationToken>()))
            .Callback<Cashier, CancellationToken>((c, t) => capturedEntity = c)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(capturedEntity);
        Assert.Equal(456, capturedEntity.EmployeeNumber);
        Assert.Equal("Pedro", capturedEntity.Name);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cashier>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_IfNotFound()
    {
        var dto = new UpdateCashierDto { Id = 999, EmployeeNumber = 1, Name = "Test", BranchId = 1, IsActive = true };

        _repositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cashier?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(dto));
    }

    [Fact]
    public async Task GetByBranchAsync_ShouldFilterByBranch()
    {
        var c1 = Cashier.Create(1, "C1", 1);
        var c2 = Cashier.Create(2, "C2", 1);
        var cashiers = new List<Cashier> { c1, c2 };

        _repositoryMock.Setup(x => x.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cashiers);

        var result = await _service.GetByBranchAsync(1);

        Assert.Equal(2, result.Count());
    }
}
