using Blanquita.Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Blanquita.Application.Queries.Supervisors;

/// <summary>
/// Query para obtener todas las encargadas (supervisores) activas.
/// </summary>
public record GetSupervisorsQuery : IRequest<IEnumerable<SupervisorDto>>;
