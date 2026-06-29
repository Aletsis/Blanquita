using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetProductSalesReport;

public class GetProductSalesReportQueryHandler : IRequestHandler<GetProductSalesReportQuery, IEnumerable<ProductSalesReportDto>>
{
    private readonly IFoxProPedidoRepository _pedidoRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IFoxProCashRegisterRepository _foxProCashRegisterRepository;

    public GetProductSalesReportQueryHandler(
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

    public async Task<IEnumerable<ProductSalesReportDto>> Handle(GetProductSalesReportQuery request, CancellationToken cancellationToken)
    {
        if (request.ProductCodes == null || !request.ProductCodes.Any())
        {
            return Enumerable.Empty<ProductSalesReportDto>();
        }

        var series = new List<string>();

        if (request.BranchId != 0)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
            if (branch == null)
            {
                return Enumerable.Empty<ProductSalesReportDto>();
            }

            // 1. Obtener series dinámicas desde FoxPro POS10041
            if (!string.IsNullOrWhiteSpace(branch.Code))
            {
                var dynamicSeries = await _foxProCashRegisterRepository.GetSeriesByBranchCodeAsync(branch.Code, cancellationToken);
                series.AddRange(dynamicSeries);
            }

            // 2. Obtener series de cajas configuradas en la base de datos de PostgreSQL
            var dbRegisters = await _cashRegisterRepository.GetByBranchAsync(request.BranchId, cancellationToken);
            var dbSeries = dbRegisters
                .Select(r => r.Serie)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            series.AddRange(dbSeries);

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
                return Enumerable.Empty<ProductSalesReportDto>();
            }
        }

        return await _pedidoRepository.GetProductSalesReportAsync(
            request.StartDate,
            request.EndDate,
            request.ProductCodes,
            series,
            cancellationToken);
    }
}

