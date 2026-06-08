using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Printing;

/// <summary>
/// Handler para el comando PrintProductLabelCommand.
/// </summary>
public class PrintProductLabelCommandHandler : IRequestHandler<PrintProductLabelCommand, bool>
{
    private readonly IPrintingService _printingService;
    private readonly IPrinterService _printerService;
    private readonly ILogger<PrintProductLabelCommandHandler> _logger;

    public PrintProductLabelCommandHandler(
        IPrintingService printingService,
        IPrinterService printerService,
        ILogger<PrintProductLabelCommandHandler> logger)
    {
        _printingService = printingService;
        _printerService = printerService;
        _logger = logger;
    }

    public async Task<bool> Handle(PrintProductLabelCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando impresión de etiqueta. Producto: {ProductCode}, Cantidad: {Quantity}, Impresora ID: {PrinterId}", 
            request.ProductCode, request.Quantity, request.PrinterId);

        try
        {
            var printer = await _printerService.GetByIdAsync(request.PrinterId);
            if (printer == null)
            {
                _logger.LogWarning("Impresora con ID {PrinterId} no encontrada", request.PrinterId);
                throw new KeyNotFoundException($"Impresora con ID {request.PrinterId} no encontrada.");
            }

            var label = new ZebraLabelDto
            {
                ProductCode = request.ProductCode,
                ProductName = request.ProductName,
                Price = request.Price,
                PrinterIp = printer.IpAddress,
                PrinterPort = printer.Port,
                PrinterDpi = printer.Dpi,
                Quantity = request.Quantity
            };

            await _printingService.PrintZebraLabelAsync(label, cancellationToken);
            _logger.LogInformation("Etiqueta impresa correctamente para {ProductCode}", request.ProductCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir etiqueta para {ProductCode}", request.ProductCode);
            throw;
        }
    }
}
