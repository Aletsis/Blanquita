using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnReport;

public record GetReturnReportQuery(int Year, int Month, string Serie) : IRequest<IEnumerable<ReturnReportItemDto>>;
