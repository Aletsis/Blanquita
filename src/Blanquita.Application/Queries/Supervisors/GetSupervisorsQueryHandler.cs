using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blanquita.Application.Queries.Supervisors;

/// <summary>
/// Handler para el query GetSupervisorsQuery.
/// </summary>
public class GetSupervisorsQueryHandler : IRequestHandler<GetSupervisorsQuery, IEnumerable<SupervisorDto>>
{
    private readonly ISupervisorService _supervisorService;
    private readonly ILogger<GetSupervisorsQueryHandler> _logger;

    public GetSupervisorsQueryHandler(
        ISupervisorService supervisorService,
        ILogger<GetSupervisorsQueryHandler> logger)
    {
        _supervisorService = supervisorService;
        _logger = logger;
    }

    public async Task<IEnumerable<SupervisorDto>> Handle(GetSupervisorsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando encargadas (supervisores) activas en el sistema");
        var supervisors = await _supervisorService.GetAllAsync(cancellationToken);
        
        // Retornamos únicamente las encargadas activas
        return supervisors.Where(s => s.IsActive).ToList();
    }
}
