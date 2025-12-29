using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.FoxPro.GetProductByCode;

/// <summary>
/// Handler para obtener un producto por su código desde FoxPro.
/// </summary>
public class GetProductByCodeQueryHandler : IRequestHandler<GetProductByCodeQuery, ProductDto?>
{
    private readonly IFoxProProductRepository _productRepository;
    private readonly ILogger<GetProductByCodeQueryHandler> _logger;

    public GetProductByCodeQueryHandler(
        IFoxProProductRepository productRepository,
        ILogger<GetProductByCodeQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductDto?> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando producto con código: {Code}", request.Code);

        var product = await _productRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Producto no encontrado con código: {Code}", request.Code);
        }
        else
        {
            _logger.LogInformation("Producto encontrado: {ProductName}", product.Name);
        }

        return product;
    }
}
