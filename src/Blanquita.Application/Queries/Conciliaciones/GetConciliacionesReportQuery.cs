using Blanquita.Application.DTOs;
using System;
using System.Collections.Generic;
using MediatR;

namespace Blanquita.Application.Queries.Conciliaciones;

/// <summary>
/// DTO que representa el reporte consolidado de conciliaciones para la UI.
/// </summary>
public record ConciliacionesReportDto(
    IEnumerable<ConciliacionCorteDto> Conciliaciones,
    int PendingBoxesCount
);

/// <summary>
/// Query para obtener el reporte consolidado y pre-ordenado de conciliaciones.
/// </summary>
public record GetConciliacionesReportQuery(
    string BranchName,
    DateTime Date
) : IRequest<ConciliacionesReportDto>;
