using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.FoxPro.GetDailyCashCuts;

/// <summary>
/// Handler para obtener cortes de caja diarios desde FoxPro.
/// </summary>
public class GetDailyCashCutsQueryHandler 
    : IRequestHandler<GetDailyCashCutsQuery, IEnumerable<CashCutDto>>
{
    private readonly IFoxProCashCutRepository _cashCutRepository;
    private readonly ILogger<GetDailyCashCutsQueryHandler> _logger;

    public GetDailyCashCutsQueryHandler(
        IFoxProCashCutRepository cashCutRepository,
        ILogger<GetDailyCashCutsQueryHandler> logger)
    {
        _cashCutRepository = cashCutRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CashCutDto>> Handle(
        GetDailyCashCutsQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Obteniendo cortes de caja para fecha: {Date}, sucursal: {BranchId}", 
            request.Date, 
            request.BranchId);

        var cashCuts = await _cashCutRepository.GetDailyCashCutsAsync(
            request.Date, 
            request.BranchId, 
            cancellationToken);

        _logger.LogInformation("Se encontraron {Count} cortes de caja", cashCuts.Count());

        return cashCuts;
    }
}
