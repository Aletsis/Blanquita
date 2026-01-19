using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.Products.SearchProducts;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IEnumerable<ProductSearchDto>>
{
    private readonly IProductCatalogRepository _repository;

    public SearchProductsQueryHandler(IProductCatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductSearchDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchAsync(request.SearchTerm, cancellationToken);
    }
}
