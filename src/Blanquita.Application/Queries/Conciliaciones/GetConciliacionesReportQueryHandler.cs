using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// Handler para el query GetConciliacionesReportQuery.
/// </summary>
public class GetConciliacionesReportQueryHandler : IRequestHandler<GetConciliacionesReportQuery, ConciliacionesReportDto>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly IBranchService _branchService;
    private readonly ILogger<GetConciliacionesReportQueryHandler> _logger;

    public GetConciliacionesReportQueryHandler(
        IConciliacionService conciliacionService,
        ICashRegisterService cashRegisterService,
        IBranchService branchService,
        ILogger<GetConciliacionesReportQueryHandler> logger)
    {
        _conciliacionService = conciliacionService;
        _cashRegisterService = cashRegisterService;
        _branchService = branchService;
        _logger = logger;
    }

    public async Task<ConciliacionesReportDto> Handle(GetConciliacionesReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando reporte de conciliaciones para la sucursal {BranchName} en la fecha {Date}", 
            request.BranchName, request.Date);

        try
        {
            var result = await _conciliacionService.GetConciliacionesByBranchAndDateAsync(request.BranchName, request.Date, cancellationToken);
            var conciliacionesList = result.ToList();

            int pendingBoxesCount = 0;
            List<ConciliacionCorteDto> sortedConciliaciones = conciliacionesList;

            // Consultar si existen cajas/cortes pendientes de conciliación en esta fecha
            var branches = await _branchService.GetAllAsync();
            var branchObj = branches.FirstOrDefault(b => string.Equals(b.Name, request.BranchName, StringComparison.OrdinalIgnoreCase));

            if (branchObj != null)
            {
                var pending = await _conciliacionService.GetAvailableBoxesAsync(request.Date, branchObj.Id, cancellationToken);
                pendingBoxesCount = pending.Count();

                // Obtener cajas configuradas en la sucursal para ordenar según su ID en PostgreSQL
                var cashRegisters = await _cashRegisterService.GetByBranchAsync(branchObj.Id);
                var registerIds = cashRegisters.ToDictionary(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);

                sortedConciliaciones = conciliacionesList
                    .OrderBy(c => registerIds.TryGetValue(c.Caja, out var id) ? id : int.MaxValue)
                    .ThenBy(c => c.Caja)
                    .ToList();
            }

            return new ConciliacionesReportDto(sortedConciliaciones, pendingBoxesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el reporte de conciliación de cortes para sucursal {BranchName} en la fecha {Date}", 
                request.BranchName, request.Date);
            throw;
        }
    }
}
