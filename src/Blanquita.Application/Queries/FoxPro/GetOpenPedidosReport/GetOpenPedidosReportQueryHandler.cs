using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetOpenPedidosReport;

public class GetOpenPedidosReportQueryHandler : IRequestHandler<GetOpenPedidosReportQuery, IEnumerable<OpenPedidoReportItemDto>>
{
    private readonly IFoxProPedidoRepository _pedidoRepository;

    public GetOpenPedidosReportQueryHandler(IFoxProPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<IEnumerable<OpenPedidoReportItemDto>> Handle(GetOpenPedidosReportQuery request, CancellationToken cancellationToken)
    {
        return await _pedidoRepository.GetOpenPedidosReportAsync(
            request.StartDate,
            request.EndDate,
            request.Series,
            request.Comanda,
            request.Ruta,
            request.SucursalNombre,
            cancellationToken);
    }
}
