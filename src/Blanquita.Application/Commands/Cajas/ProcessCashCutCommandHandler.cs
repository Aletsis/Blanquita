using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blanquita.Application.Commands.Cajas;

/// <summary>
/// Handler para el comando ProcessCashCutCommand.
/// </summary>
public class ProcessCashCutCommandHandler : IRequestHandler<ProcessCashCutCommand, ProcessCashCutResult>
{
    private readonly ICashCutService _cashCutService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly IPrintingService _printingService;
    private readonly ILogger<ProcessCashCutCommandHandler> _logger;

    public ProcessCashCutCommandHandler(
        ICashCutService cashCutService,
        ICashRegisterService cashRegisterService,
        IPrintingService printingService,
        ILogger<ProcessCashCutCommandHandler> logger)
    {
        _cashCutService = cashCutService;
        _cashRegisterService = cashRegisterService;
        _printingService = printingService;
        _logger = logger;
    }

    public async Task<ProcessCashCutResult> Handle(ProcessCashCutCommand request, CancellationToken cancellationToken)
    {
        var pRequest = request.Request;
        _logger.LogInformation("Iniciando procesamiento de corte de caja. Supervisor: {SupervisorId}, Cajera: {CashierId}, Caja: {CashRegisterId}", 
            pRequest.SupervisorId, pRequest.CashierId, pRequest.CashRegisterId);

        try
        {
            // 1. Procesar el corte mediante el servicio de negocio
            var savedCut = await _cashCutService.ProcessCashCutAsync(pRequest, cancellationToken);
            _logger.LogInformation("Corte de caja guardado con éxito. ID: {CutId}", savedCut.Id);

            // 2. Obtener la información de la caja registradora para resolver la impresora
            var register = await _cashRegisterService.GetByIdAsync(pRequest.CashRegisterId, cancellationToken);
            if (register == null)
            {
                _logger.LogWarning("No se encontró la caja registradora con ID {CashRegisterId} para resolver la impresora", pRequest.CashRegisterId);
                return new ProcessCashCutResult(true, savedCut, false, "El corte se guardó correctamente, pero no se encontró la impresora.");
            }

            // 3. Ejecutar la impresión física
            bool printingSucceeded = false;
            string? message = null;
            try
            {
                _logger.LogInformation("Enviando corte a imprimir a la dirección {PrinterIp}:{PrinterPort}", register.PrinterIp, register.PrinterPort);
                await _printingService.PrintCashCutAsync(savedCut, register.PrinterIp, register.PrinterPort, cancellationToken);
                printingSucceeded = true;
                _logger.LogInformation("Impresión física de corte completada para ID {CutId}", savedCut.Id);
            }
            catch (Exception printEx)
            {
                _logger.LogError(printEx, "Error al imprimir corte físico de caja para ID {CutId}", savedCut.Id);
                message = "El corte se guardó correctamente, pero falló la conexión con la impresora.";
            }

            return new ProcessCashCutResult(true, savedCut, printingSucceeded, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar el proceso de corte de caja");
            return new ProcessCashCutResult(false, null, false, ex.Message);
        }
    }
}
