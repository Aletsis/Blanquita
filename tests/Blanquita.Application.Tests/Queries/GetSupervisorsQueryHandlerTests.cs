using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Queries.Supervisors;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blanquita.Application.Tests.Queries;

public class GetSupervisorsQueryHandlerTests
{
    private readonly Mock<ISupervisorService> _mockSupervisorService;
    private readonly Mock<ILogger<GetSupervisorsQueryHandler>> _mockLogger;
    private readonly GetSupervisorsQueryHandler _handler;

    public GetSupervisorsQueryHandlerTests()
    {
        _mockSupervisorService = new Mock<ISupervisorService>();
        _mockLogger = new Mock<ILogger<GetSupervisorsQueryHandler>>();
        _handler = new GetSupervisorsQueryHandler(_mockSupervisorService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_RetrievesOnlyActiveSupervisors()
    {
        // Arrange
        var supervisors = new List<SupervisorDto>
        {
            new SupervisorDto { Id = 1, Name = "Supervisor Activo", IsActive = true },
            new SupervisorDto { Id = 2, Name = "Supervisor Inactivo", IsActive = false }
        };

        _mockSupervisorService
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(supervisors);

        var query = new GetSupervisorsQuery();

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Supervisor Activo", result[0].Name);
        _mockSupervisorService.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
