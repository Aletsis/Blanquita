using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetDailyCashCuts;

/// <summary>
/// Query para obtener cortes de caja diarios desde FoxPro.
/// </summary>
public record GetDailyCashCutsQuery(DateTime Date, int BranchId) : IRequest<IEnumerable<CashCutDto>>;
