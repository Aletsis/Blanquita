using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// Handler para el query GetAvailableBoxesQuery.
/// </summary>
public class GetAvailableBoxesQueryHandler : IRequestHandler<GetAvailableBoxesQuery, IEnumerable<AvailableBoxDto>>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly ILogger<GetAvailableBoxesQueryHandler> _logger;

    public GetAvailableBoxesQueryHandler(
        IConciliacionService conciliacionService,
        ILogger<GetAvailableBoxesQueryHandler> _logger)
    {
        _conciliacionService = conciliacionService;
        this._logger = _logger;
    }

    public async Task<IEnumerable<AvailableBoxDto>> Handle(GetAvailableBoxesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando cajas disponibles para fecha {Date} y sucursal {BranchId}", request.Date, request.BranchId);
        return await _conciliacionService.GetAvailableBoxesAsync(request.Date, request.BranchId, cancellationToken);
    }
}
