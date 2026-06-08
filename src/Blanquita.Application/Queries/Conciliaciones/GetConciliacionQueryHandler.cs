using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// Handler para el query GetConciliacionQuery.
/// </summary>
public class GetConciliacionQueryHandler : IRequestHandler<GetConciliacionQuery, ConciliacionResultDto>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly ILogger<GetConciliacionQueryHandler> _logger;

    public GetConciliacionQueryHandler(
        IConciliacionService conciliacionService,
        ILogger<GetConciliacionQueryHandler> logger)
    {
        _conciliacionService = conciliacionService;
        _logger = logger;
    }

    public async Task<ConciliacionResultDto> Handle(GetConciliacionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando conciliación para Caja {BoxId}, Turno {ShiftId}, Cajero {CashierId}", 
            request.CashRegisterId, request.ShiftId, request.CashierId);
            
        return await _conciliacionService.GetConciliacionAsync(
            request.CashRegisterId,
            request.ShiftId,
            request.CashierId,
            request.Date,
            cancellationToken);
    }
}
