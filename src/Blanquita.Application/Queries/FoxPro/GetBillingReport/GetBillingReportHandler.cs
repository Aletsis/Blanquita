using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetBillingReport;

public class GetBillingReportHandler : IRequestHandler<GetBillingReportQuery, IEnumerable<BillingReportItemDto>>
{
    private readonly IFoxProDocumentRepository _documentRepository;

    public GetBillingReportHandler(IFoxProDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<BillingReportItemDto>> Handle(GetBillingReportQuery request, CancellationToken cancellationToken)
    {
        return await _documentRepository.GetBillingReportAsync(request.Date, request.Serie, cancellationToken);
    }
}
