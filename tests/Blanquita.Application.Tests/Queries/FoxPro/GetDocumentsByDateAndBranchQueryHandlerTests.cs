using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetDocumentsByDateAndBranch;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetDocumentsByDateAndBranchQueryHandlerTests
{
    private readonly Mock<IFoxProDocumentRepository> _mockRepository;
    private readonly Mock<ILogger<GetDocumentsByDateAndBranchQueryHandler>> _mockLogger;
    private readonly GetDocumentsByDateAndBranchQueryHandler _handler;

    public GetDocumentsByDateAndBranchQueryHandlerTests()
    {
        _mockRepository = new Mock<IFoxProDocumentRepository>();
        _mockLogger = new Mock<ILogger<GetDocumentsByDateAndBranchQueryHandler>>();
        _handler = new GetDocumentsByDateAndBranchQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_DocumentsExist_ReturnsDocuments()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;
        var expectedDocuments = new List<DocumentDto>
        {
            new DocumentDto
            {
                DocumentNumber = "DOC001",
                Date = date,
                Total = 100.50m,
                Serie = "A",
                Folio = "1",
                CajaTexto = "Caja 1"
            },
            new DocumentDto
            {
                DocumentNumber = "DOC002",
                Date = date,
                Total = 200.75m,
                Serie = "A",
                Folio = "2",
                CajaTexto = "Caja 2"
            }
        };

        _mockRepository
            .Setup(r => r.GetByDateAndBranchAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDocuments);

        var query = new GetDocumentsByDateAndBranchQuery(date, branchId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedDocuments[0].DocumentNumber, resultList[0].DocumentNumber);
        Assert.Equal(expectedDocuments[1].Total, resultList[1].Total);
        Assert.Equal(expectedDocuments[0].Serie, resultList[0].Serie);

        _mockRepository.Verify(r => r.GetByDateAndBranchAsync(date, branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoDocumentsFound_ReturnsEmptyList()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;

        _mockRepository
            .Setup(r => r.GetByDateAndBranchAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto>());

        var query = new GetDocumentsByDateAndBranchQuery(date, branchId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetByDateAndBranchAsync(date, branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var date = new DateTime(2025, 12, 29);
        var branchId = 1;
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository
            .Setup(r => r.GetByDateAndBranchAsync(date, branchId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new GetDocumentsByDateAndBranchQuery(date, branchId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }
}
