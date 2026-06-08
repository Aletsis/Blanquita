using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetPedidoBySeriesAndFolio;

public class GetPedidoBySeriesAndFolioQueryHandler : IRequestHandler<GetPedidoBySeriesAndFolioQuery, PedidoDto?>
{
    private readonly IFoxProPedidoRepository _repository;

    public GetPedidoBySeriesAndFolioQueryHandler(IFoxProPedidoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PedidoDto?> Handle(GetPedidoBySeriesAndFolioQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetBySeriesAndFolioAsync(request.Series, request.Folio, cancellationToken);
    }
}
