using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetSalesReportByFilters;

public class GetSalesReportByFiltersQueryHandler : IRequestHandler<GetSalesReportByFiltersQuery, IEnumerable<PedidoDto>>
{
    private readonly IFoxProPedidoRepository _pedidoRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IFoxProCashRegisterRepository _foxProCashRegisterRepository;

    public GetSalesReportByFiltersQueryHandler(
        IFoxProPedidoRepository pedidoRepository,
        ICashRegisterRepository cashRegisterRepository,
        IBranchRepository branchRepository,
        IFoxProCashRegisterRepository foxProCashRegisterRepository)
    {
        _pedidoRepository = pedidoRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _branchRepository = branchRepository;
        _foxProCashRegisterRepository = foxProCashRegisterRepository;
    }

    public async Task<IEnumerable<PedidoDto>> Handle(GetSalesReportByFiltersQuery request, CancellationToken cancellationToken)
    {
        var series = new List<string>();

        if (request.BranchId != 0)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
            if (branch == null)
            {
                return Enumerable.Empty<PedidoDto>();
            }

            // 1. Obtener series dinámicas desde FoxPro POS10041
            if (!string.IsNullOrWhiteSpace(branch.Code))
            {
                var dynamicSeries = await _foxProCashRegisterRepository.GetSeriesByBranchCodeAsync(branch.Code, cancellationToken);
                series.AddRange(dynamicSeries);
            }

            // 2. Obtener series de cajas configuradas en la base de datos de PostgreSQL
            var dbRegisters = (await _cashRegisterRepository.GetByBranchAsync(request.BranchId, cancellationToken)).ToList();
            var dbSeries = dbRegisters
                .Select(r => r.Serie)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            series.AddRange(dbSeries);

            // Resolver también la serie (CSERIENOTA) en POS10041 mediante el IdContpaqi (CIDCAJA) de cada caja
            foreach (var register in dbRegisters.Where(r => r.IdContpaqi > 0))
            {
                var foxProSerie = await _foxProCashRegisterRepository.GetNameByIdAsync(register.IdContpaqi, cancellationToken);
                if (!string.IsNullOrWhiteSpace(foxProSerie))
                {
                    series.Add(foxProSerie);
                }
            }

            // 3. Obtener series propias de la sucursal (Cliente, Global, Devolución)
            if (!string.IsNullOrWhiteSpace(branch.SeriesCliente)) series.Add(branch.SeriesCliente);
            if (!string.IsNullOrWhiteSpace(branch.SeriesGlobal)) series.Add(branch.SeriesGlobal);
            if (!string.IsNullOrWhiteSpace(branch.SeriesDevolucion)) series.Add(branch.SeriesDevolucion);

            series = series
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!series.Any())
            {
                return Enumerable.Empty<PedidoDto>();
            }
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
