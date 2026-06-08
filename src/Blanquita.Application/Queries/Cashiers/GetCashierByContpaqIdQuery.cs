using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.Cashiers;

/// <summary>
/// Query para obtener/auto-resolver una cajera por su ID de Contpaqi y el ID de su sucursal.
/// </summary>
public record GetCashierByContpaqIdQuery(int BranchId, int ContpaqId) : IRequest<CashierDto?>;
