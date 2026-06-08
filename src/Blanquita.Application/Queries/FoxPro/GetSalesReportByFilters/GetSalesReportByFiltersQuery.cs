using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetSalesReportByFilters;

public record GetSalesReportByFiltersQuery(
    DateTime StartDate,
    DateTime EndDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int BranchId) : IRequest<IEnumerable<PedidoDto>>;
