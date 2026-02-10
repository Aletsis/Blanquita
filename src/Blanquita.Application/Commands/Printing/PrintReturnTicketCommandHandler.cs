using Blanquita.Application.Constants;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using Blanquita.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Printing;

public class PrintReturnTicketCommandHandler : IRequestHandler<PrintReturnTicketCommand, bool>
{
    private readonly IPrintingService _printingService;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<PrintReturnTicketCommandHandler> _logger;

    public PrintReturnTicketCommandHandler(
        IPrintingService printingService,
        ICashRegisterRepository cashRegisterRepository,
        IConfiguracionService configuracionService,
        ILogger<PrintReturnTicketCommandHandler> logger)
    {
        _printingService = printingService;
        _cashRegisterRepository = cashRegisterRepository;
        _configuracionService = configuracionService;
        _logger = logger;
    }

    public async Task<bool> Handle(PrintReturnTicketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            string ip;
            int port;

            if (request.CashRegisterId > 0)
            {
                var register = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
                if (register != null && register.PrinterConfig != null)
                {
                    ip = register.PrinterConfig.IpAddress;
                    port = register.PrinterConfig.Port;
                }
                else
                {
                    _logger.LogWarning("CashRegister {RegisterId} not found or printer not configured, using default.", request.CashRegisterId);
                    ip = config.PrinterIp;
                    port = config.PrinterPort;
                }
            }
            else
            {
                // Default to primary
                ip = config.PrinterIp;
                port = config.PrinterPort;
            }

            if (string.IsNullOrEmpty(ip) || port <= 0)
            {
                _logger.LogWarning("Printer not configured correctly.");
                return false;
            }

            await _printingService.PrintReturnTicketAsync(request.ReturnDto, ip, port, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PrintReturnTicketCommand");
            return false; // Or throw depending on preference
        }
    }
}
