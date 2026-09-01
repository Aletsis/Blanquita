using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnReport;

public record GetReturnReportQuery(
    int? Year = null, 
    int? Month = null, 
    string? Serie = null, 
    DateTime? StartDate = null, 
    DateTime? EndDate = null, 
    string? Tipo = null
) : IRequest<IEnumerable<ReturnReportItemDto>>;
