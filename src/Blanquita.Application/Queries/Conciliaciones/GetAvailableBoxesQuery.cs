using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// Query para obtener las cajas disponibles con apertura diaria para conciliación.
/// </summary>
public record GetAvailableBoxesQuery(DateTime Date, int? BranchId = null) : IRequest<IEnumerable<AvailableBoxDto>>;
