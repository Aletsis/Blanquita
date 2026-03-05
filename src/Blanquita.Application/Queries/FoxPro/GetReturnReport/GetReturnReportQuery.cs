using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnReport;

public record GetReturnReportQuery(DateTime Date, string Serie) : IRequest<IEnumerable<ReturnReportItemDto>>;
