using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Printing;

public class PrintPedidoTicketCommand : IRequest<bool>
{
    public PedidoDto Pedido { get; }
    public int CashRegisterId { get; }

    public PrintPedidoTicketCommand(PedidoDto pedido, int cashRegisterId)
    {
        Pedido = pedido;
        CashRegisterId = cashRegisterId;
    }
}
