using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetProductByCode;

/// <summary>
/// Query para obtener un producto por su código desde FoxPro.
/// </summary>
public record GetProductByCodeQuery(string Code) : IRequest<ProductDto?>;
