using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetCancellationReport;

public class GetCancellationReportQueryHandler : IRequestHandler<GetCancellationReportQuery, IEnumerable<CancellationReportItemDto>>
{
    private readonly IFoxProDocumentRepository _documentRepository;

    public GetCancellationReportQueryHandler(IFoxProDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<CancellationReportItemDto>> Handle(GetCancellationReportQuery request, CancellationToken cancellationToken)
    {
        return await _documentRepository.GetCancellationsReportAsync(
            request.StartDate,
            request.EndDate,
            request.Serie,
            request.Tipo,
            cancellationToken);
    }
}
