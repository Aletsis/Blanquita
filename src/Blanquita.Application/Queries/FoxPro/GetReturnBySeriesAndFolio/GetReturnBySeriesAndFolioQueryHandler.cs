using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnBySeriesAndFolio;

/// <summary>
/// Handler for GetReturnBySeriesAndFolioQuery.
/// </summary>
public class GetReturnBySeriesAndFolioQueryHandler : IRequestHandler<GetReturnBySeriesAndFolioQuery, ReturnDto?>
{
    private readonly IReturnRepository _repository;

    public GetReturnBySeriesAndFolioQueryHandler(IReturnRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReturnDto?> Handle(GetReturnBySeriesAndFolioQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetBySeriesAndFolioAsync(request.Series, request.Folio, cancellationToken);
    }
}
