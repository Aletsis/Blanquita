using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.Cajas;

/// <summary>
/// Query para obtener el estado y métricas en tiempo real de las cajas con apertura en una fecha dada.
/// </summary>
public record GetMonitorCajasQuery(DateTime Date, int? BranchId = null) : IRequest<IEnumerable<CajaMonitorDto>>;
