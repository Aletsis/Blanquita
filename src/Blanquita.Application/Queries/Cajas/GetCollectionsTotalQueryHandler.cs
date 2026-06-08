using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blanquita.Application.Queries.Cajas;

/// <summary>
/// Handler para el query GetCollectionsTotalQuery.
/// </summary>
public class GetCollectionsTotalQueryHandler : IRequestHandler<GetCollectionsTotalQuery, decimal>
{
    private readonly ICashCollectionService _cashCollectionService;
    private readonly ILogger<GetCollectionsTotalQueryHandler> _logger;

    public GetCollectionsTotalQueryHandler(
        ICashCollectionService cashCollectionService,
        ILogger<GetCollectionsTotalQueryHandler> logger)
    {
        _cashCollectionService = cashCollectionService;
        _logger = logger;
    }

    public async Task<decimal> Handle(GetCollectionsTotalQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculando total de recolecciones para la caja {CashRegisterName} en la fecha {Date}", 
            request.CashRegisterName, request.Date);

        try
        {
            var startOfDay = request.Date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var searchRequest = new SearchCashCollectionRequest
            {
                FechaInicio = startOfDay,
                FechaFin = endOfDay,
                CashRegisterName = request.CashRegisterName,
                IsCut = false
            };

            var collections = await _cashCollectionService.SearchAsync(searchRequest, cancellationToken);

            var total = Helpers.FinancialCalculator.CalculateCollectionsTotal(collections);

            _logger.LogInformation("Total de recolecciones calculado para la caja {CashRegisterName}: {Total}", 
                request.CashRegisterName, total);

            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular el total de recolecciones para la caja {CashRegisterName}", request.CashRegisterName);
            throw;
        }
    }
}
