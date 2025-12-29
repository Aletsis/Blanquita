using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Services;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class SupervisorServiceTests
{
    private readonly Mock<ISupervisorRepository> _repositoryMock;
    private readonly SupervisorService _service;

    public SupervisorServiceTests()
    {
        _repositoryMock = new Mock<ISupervisorRepository>();
        _service = new SupervisorService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        var entity = Supervisor.Create("Juan", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, 10);

        _repositoryMock.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Juan", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_IfNameExists()
    {
        var dto = new CreateSupervisorDto { Name = "Duplicate", BranchId = 1 };

        _repositoryMock.Setup(x => x.ExistsAsync("Duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepo_WhenValid()
    {
        var dto = new CreateSupervisorDto { Name = "Pedro", BranchId = 2, IsActive = true };

        _repositoryMock.Setup(x => x.ExistsAsync("Pedro", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Supervisor? capturedEntity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Supervisor>(), It.IsAny<CancellationToken>()))
            .Callback<Supervisor, CancellationToken>((s, t) => capturedEntity = s)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(capturedEntity);
        Assert.Equal("Pedro", capturedEntity.Name);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Supervisor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_IfNotFound()
    {
        var dto = new UpdateSupervisorDto { Id = 999, Name = "Test", BranchId = 1, IsActive = true };

        _repositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supervisor?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(dto));
    }

    [Fact]
    public async Task GetByBranchAsync_ShouldFilterByBranch()
    {
        var s1 = Supervisor.Create("S1", 1);
        var s2 = Supervisor.Create("S2", 1);
        var supervisors = new List<Supervisor> { s1, s2 };

        _repositoryMock.Setup(x => x.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supervisors);

        var result = await _service.GetByBranchAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActive()
    {
        var s1 = Supervisor.Create("S1", 1, true);
        var supervisors = new List<Supervisor> { s1 };

        _repositoryMock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(supervisors);

        var result = await _service.GetActiveAsync();

        Assert.Single(result);
    }
}
