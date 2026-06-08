using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetSalesReportByFilters;

public class GetSalesReportByFiltersQueryHandler : IRequestHandler<GetSalesReportByFiltersQuery, IEnumerable<PedidoDto>>
{
    private readonly IFoxProPedidoRepository _pedidoRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;

    public GetSalesReportByFiltersQueryHandler(
        IFoxProPedidoRepository pedidoRepository,
        ICashRegisterRepository cashRegisterRepository)
    {
        _pedidoRepository = pedidoRepository;
        _cashRegisterRepository = cashRegisterRepository;
    }

    public async Task<IEnumerable<PedidoDto>> Handle(GetSalesReportByFiltersQuery request, CancellationToken cancellationToken)
    {
        var registers = await _cashRegisterRepository.GetByBranchAsync(request.BranchId, cancellationToken);
        
        var series = registers
            .Select(r => r.Serie)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        if (!series.Any())
        {
            return Enumerable.Empty<PedidoDto>();
        }

        return await _pedidoRepository.GetSalesReportByFiltersAsync(
            request.StartDate,
            request.EndDate,
            request.StartTime,
            request.EndTime,
            series,
            cancellationToken);
    }
}
