using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Printing;

public class PrintPedidoTicketCommandHandler : IRequestHandler<PrintPedidoTicketCommand, bool>
{
    private readonly IPrintingService _printingService;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ILogger<PrintPedidoTicketCommandHandler> _logger;

    public PrintPedidoTicketCommandHandler(
        IPrintingService printingService,
        ICashRegisterRepository cashRegisterRepository,
        ILogger<PrintPedidoTicketCommandHandler> logger)
    {
        _printingService = printingService;
        _cashRegisterRepository = cashRegisterRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(PrintPedidoTicketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cashRegister = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId);
            
            if (cashRegister == null || cashRegister.PrinterConfig == null)
            {
                _logger.LogWarning("Printer configuration not found for cash register {CashRegisterId}", request.CashRegisterId);
                return false;
            }

            var printerIp = cashRegister.PrinterConfig.IpAddress;
            var printerPort = cashRegister.PrinterConfig.Port;

            await _printingService.PrintPedidoTicketAsync(request.Pedido, printerIp, printerPort, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing pedido ticket for {Folio}", request.Pedido.Folio);
            return false;
        }
    }
}
