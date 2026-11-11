using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class BranchServiceTests
{
    private readonly Mock<IBranchRepository> _repositoryMock;
    private readonly Mock<ILogger<BranchService>> _loggerMock;
    private readonly BranchService _service;

    public BranchServiceTests()
    {
        _repositoryMock = new Mock<IBranchRepository>();
        _loggerMock = new Mock<ILogger<BranchService>>();
        _service = new BranchService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBranches()
    {
        var branches = new List<Branch> 
        { 
            Branch.Create("Himno Nacional", "01", "C", "G", "D", "", null), 
            Branch.Create("Pozos", "02", "C", "G", "D", "", null) 
        };
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        var result = await _service.GetAllAsync();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBranch_WhenExists()
    {
        var branch = Branch.Create("Himno Nacional", "01", "C", "G", "D", "", null);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(branch, 6);

        _repositoryMock.Setup(x => x.GetByIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var result = await _service.GetByIdAsync(6);
        
        Assert.NotNull(result);
        Assert.Equal("Himno Nacional", result.Name);
        Assert.Equal(6, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _repositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.GetByIdAsync(999);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPermiteSalidasEfectivo()
    {
        var dto = new BranchDto
        {
            Name = "Sucursal Nueva",
            Code = "03",
            PermiteSalidasEfectivo = true
        };

        var result = await _service.CreateAsync(dto);

        Assert.True(result.PermiteSalidasEfectivo);
        _repositoryMock.Verify(x => x.AddAsync(
            It.Is<Branch>(b => b.Name == "Sucursal Nueva" && b.PermiteSalidasEfectivo),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePermiteSalidasEfectivo()
    {
        var branch = Branch.Create("Himno Nacional", "01", "C", "G", "D", "", null, false);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(branch, 6);
        _repositoryMock.Setup(x => x.GetByIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var dto = new BranchDto
        {
            Id = 6,
            Name = "Himno Nacional",
            Code = "01",
            SeriesCliente = "C",
            SeriesGlobal = "G",
            SeriesDevolucion = "D",
            Direccion = "",
            PermiteSalidasEfectivo = true
        };

        await _service.UpdateAsync(dto);

        Assert.True(branch.PermiteSalidasEfectivo);
        _repositoryMock.Verify(x => x.UpdateAsync(branch, It.IsAny<CancellationToken>()), Times.Once);
    }
}
