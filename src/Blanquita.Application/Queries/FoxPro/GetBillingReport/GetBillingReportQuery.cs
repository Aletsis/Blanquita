using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetBillingReport;

public record GetBillingReportQuery(DateTime Date, string Serie) : IRequest<IEnumerable<BillingReportItemDto>>;
