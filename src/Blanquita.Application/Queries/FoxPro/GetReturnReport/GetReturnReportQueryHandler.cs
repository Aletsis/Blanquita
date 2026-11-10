using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnReport;

public class GetReturnReportQueryHandler : IRequestHandler<GetReturnReportQuery, IEnumerable<ReturnReportItemDto>>
{
    private readonly IFoxProDocumentRepository _foxProRepository;
    private readonly IBranchRepository? _branchRepository;
    private readonly ICashRegisterRepository? _cashRegisterRepository;
    private readonly IFoxProCashRegisterRepository? _foxProCashRegisterRepository;

    public GetReturnReportQueryHandler(
        IFoxProDocumentRepository foxProRepository,
        IBranchRepository? branchRepository = null,
        ICashRegisterRepository? cashRegisterRepository = null,
        IFoxProCashRegisterRepository? foxProCashRegisterRepository = null)
    {
        _foxProRepository = foxProRepository;
        _branchRepository = branchRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _foxProCashRegisterRepository = foxProCashRegisterRepository;
    }

    public async Task<IEnumerable<ReturnReportItemDto>> Handle(GetReturnReportQuery request, CancellationToken cancellationToken)
    {
        if (!request.BranchId.HasValue || request.BranchId.Value <= 0 || _branchRepository == null)
        {
            if (request.Series != null && request.Series.Any())
            {
                return await _foxProRepository.GetReturnsReportAsync(
                    request.Year,
                    request.Month,
                    request.Serie,
                    request.StartDate,
                    request.EndDate,
                    request.Tipo,
                    cancellationToken,
                    request.Series);
            }

            return await _foxProRepository.GetReturnsReportAsync(
                request.Year, 
                request.Month, 
                request.Serie, 
                request.StartDate, 
                request.EndDate, 
                request.Tipo, 
                cancellationToken);
        }

        var series = new List<string>();

        if (request.Series != null)
        {
            series.AddRange(request.Series);
        }

        if (!string.IsNullOrWhiteSpace(request.Serie))
        {
            series.Add(request.Serie);
        }

        var branch = await _branchRepository.GetByIdAsync(request.BranchId.Value, cancellationToken);
        if (branch != null)
        {
            // 1. Obtener series dinámicas desde FoxPro POS10041
            if (!string.IsNullOrWhiteSpace(branch.Code) && _foxProCashRegisterRepository != null)
            {
                var dynamicSeries = await _foxProCashRegisterRepository.GetSeriesByBranchCodeAsync(branch.Code, cancellationToken);
                series.AddRange(dynamicSeries);
            }

            // 2. Obtener series de cajas configuradas en PostgreSQL
            if (_cashRegisterRepository != null)
            {
                var dbRegisters = (await _cashRegisterRepository.GetByBranchAsync(request.BranchId.Value, cancellationToken)).ToList();
                var dbSeries = dbRegisters
                    .Select(r => r.Serie)
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                series.AddRange(dbSeries);

                if (_foxProCashRegisterRepository != null)
                {
                    foreach (var register in dbRegisters.Where(r => r.IdContpaqi > 0))
                    {
                        var foxProSerie = await _foxProCashRegisterRepository.GetNameByIdAsync(register.IdContpaqi, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(foxProSerie))
                        {
                            series.Add(foxProSerie);
                        }
                    }
                }
            }

            // 3. Obtener series propias de la sucursal (Cliente, Global, Devolución)
            if (!string.IsNullOrWhiteSpace(branch.SeriesCliente)) series.Add(branch.SeriesCliente);
            if (!string.IsNullOrWhiteSpace(branch.SeriesGlobal)) series.Add(branch.SeriesGlobal);
            if (!string.IsNullOrWhiteSpace(branch.SeriesDevolucion)) series.Add(branch.SeriesDevolucion);
        }

        var distinctSeries = series
            .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return await _foxProRepository.GetReturnsReportAsync(
            request.Year,
            request.Month,
            serie: null,
            request.StartDate,
            request.EndDate,
            request.Tipo,
            cancellationToken,
            series: distinctSeries);
    }
}
