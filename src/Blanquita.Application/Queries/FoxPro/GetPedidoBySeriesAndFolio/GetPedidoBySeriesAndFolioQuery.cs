using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetPedidoBySeriesAndFolio;

public class GetPedidoBySeriesAndFolioQuery : IRequest<PedidoDto?>
{
    public string Series { get; }
    public string Folio { get; }

    public GetPedidoBySeriesAndFolioQuery(string series, string folio)
    {
        Series = series;
        Folio = folio;
    }
}
