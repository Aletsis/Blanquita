using Blanquita.Application.Interfaces;
using MediatR;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// Query para obtener el resumen de ventas y recolecciones de una caja para conciliar.
/// </summary>
public record GetConciliacionQuery(
    int CashRegisterId,
    int ShiftId,
    int CashierId,
    DateTime Date
) : IRequest<ConciliacionResultDto>;
