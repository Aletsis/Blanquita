using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.Products.SearchProducts;

/// <summary>
/// Query agnóstica para buscar productos en el catálogo
/// </summary>
public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductSearchDto>>;
