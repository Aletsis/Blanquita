using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetProductByCode;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetProductByCodeQueryHandlerTests
{
    private readonly Mock<IProductCatalogRepository> _mockRepository;
    private readonly Mock<ILogger<GetProductByCodeQueryHandler>> _mockLogger;
    private readonly GetProductByCodeQueryHandler _handler;

    public GetProductByCodeQueryHandlerTests()
    {
        _mockRepository = new Mock<IProductCatalogRepository>();
        _mockLogger = new Mock<ILogger<GetProductByCodeQueryHandler>>();
        _handler = new GetProductByCodeQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ProductExists_ReturnsProduct()
    {
        // Arrange
        var code = "TEST123";
        var expectedProduct = new ProductDto
        {
            Code = code,
            Name = "Test Product",
            BasePrice = 10.50m,
            TaxRate = 16m
        };

        _mockRepository
            .Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        var query = new GetProductByCodeQuery(code);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProduct.Code, result.Code);
        Assert.Equal(expectedProduct.Name, result.Name);
        Assert.Equal(expectedProduct.BasePrice, result.BasePrice);
        Assert.Equal(expectedProduct.TaxRate, result.TaxRate);
        Assert.Equal(expectedProduct.PriceWithTax, result.PriceWithTax);

        _mockRepository.Verify(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsNull()
    {
        // Arrange
        var code = "NONEXISTENT";

        _mockRepository
            .Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        var query = new GetProductByCodeQuery(code);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);

        _mockRepository.Verify(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyCode_ReturnsNull()
    {
        // Arrange
        var query = new GetProductByCodeQuery("");

        _mockRepository
            .Setup(r => r.GetByCodeAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var code = "ERROR123";
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository
            .Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new GetProductByCodeQuery(code);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var code = "TEST123";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new GetProductByCodeQuery(code);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }
}
