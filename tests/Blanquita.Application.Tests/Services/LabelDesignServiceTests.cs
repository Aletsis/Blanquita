using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class LabelDesignServiceTests
{
    private readonly Mock<ILabelDesignRepository> _repositoryMock;
    private readonly Mock<ILogger<LabelDesignService>> _loggerMock;
    private readonly LabelDesignService _service;

    public LabelDesignServiceTests()
    {
        _repositoryMock = new Mock<ILabelDesignRepository>();
        _loggerMock = new Mock<ILogger<LabelDesignService>>();
        
        _service = new LabelDesignService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddDesignToDb()
    {
        var dto = new LabelDesignDto
        {
            Name = "New Design",
            WidthInMm = 50,
            HeightInMm = 20,
            IsDefault = true
        };

        _repositoryMock.Setup(x => x.ExistsAsync("New Design", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<LabelDesign>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.RemoveAllDefaultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_IfNameExists()
    {
        _repositoryMock.Setup(x => x.ExistsAsync("Duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new LabelDesignDto { Name = "Duplicate" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task SetAsDefaultAsync_ShouldUnsetOthers()
    {
        var d2 = LabelDesign.Create("D2", 50, 20, isDefault: false);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(d2, 2);

        _repositoryMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(d2);

        // Act: Set D2 as default
        await _service.SetAsDefaultAsync(2);

        _repositoryMock.Verify(x => x.RemoveAllDefaultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<LabelDesign>(d => d.IsDefault), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFromDb()
    {
        var d1 = LabelDesign.Create("D1", 50, 20);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(d1, 1);
        
        _repositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(d1);

        await _service.DeleteAsync(1);

        _repositoryMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
