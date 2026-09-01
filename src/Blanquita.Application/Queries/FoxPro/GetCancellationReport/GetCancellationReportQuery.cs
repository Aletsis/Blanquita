using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetCancellationReport;

public record GetCancellationReportQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Serie = null,
    string? Tipo = null
) : IRequest<IEnumerable<CancellationReportItemDto>>;
