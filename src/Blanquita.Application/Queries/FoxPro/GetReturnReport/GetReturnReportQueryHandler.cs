using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnReport;

public class GetReturnReportQueryHandler : IRequestHandler<GetReturnReportQuery, IEnumerable<ReturnReportItemDto>>
{
    private readonly IFoxProDocumentRepository _foxProRepository;

    public GetReturnReportQueryHandler(IFoxProDocumentRepository foxProRepository)
    {
        _foxProRepository = foxProRepository;
    }

    public async Task<IEnumerable<ReturnReportItemDto>> Handle(GetReturnReportQuery request, CancellationToken cancellationToken)
    {
        return await _foxProRepository.GetReturnsReportAsync(request.Year, request.Month, request.Serie, cancellationToken);
    }
}
