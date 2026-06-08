using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blanquita.Application.Commands.Cajas;

/// <summary>
/// Handler para el comando CreateCashCollectionCommand.
/// </summary>
public class CreateCashCollectionCommandHandler : IRequestHandler<CreateCashCollectionCommand, CreateCashCollectionResult>
{
    private readonly ICashCollectionService _cashCollectionService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly IPrintingService _printingService;
    private readonly ILogger<CreateCashCollectionCommandHandler> _logger;

    public CreateCashCollectionCommandHandler(
        ICashCollectionService cashCollectionService,
        ICashRegisterService cashRegisterService,
        IPrintingService printingService,
        ILogger<CreateCashCollectionCommandHandler> logger)
    {
        _cashCollectionService = cashCollectionService;
        _cashRegisterService = cashRegisterService;
        _printingService = printingService;
        _logger = logger;
    }

    public async Task<CreateCashCollectionResult> Handle(CreateCashCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de recolección de efectivo para la caja ID {CashRegisterId}", request.CashRegisterId);

        try
        {
            // 1. Guardar la recolección física de efectivo
            var collection = await _cashCollectionService.CreateAsync(request.Dto, cancellationToken);
            _logger.LogInformation("Recolección de efectivo guardada con éxito. ID: {CollectionId}, Folio: {Folio}", collection.Id, collection.Folio);

            // 2. Obtener la información de la caja registradora principal
            var register = await _cashRegisterService.GetByIdAsync(request.CashRegisterId, cancellationToken);
            if (register == null)
            {
                _logger.LogWarning("No se encontró la caja registradora con ID {CashRegisterId} para resolver la impresora", request.CashRegisterId);
                return new CreateCashCollectionResult(true, collection, false, false, "Recolección guardada pero no se pudo resolver la impresora.");
            }

            // 3. Intentar imprimir en la impresora principal
            try
            {
                _logger.LogInformation("Intentando imprimir recolección en impresora principal {PrinterIp}:{PrinterPort}", register.PrinterIp, register.PrinterPort);
                await _printingService.PrintCashCollectionAsync(collection, register.PrinterIp, register.PrinterPort, cancellationToken);
                _logger.LogInformation("Impresión física de recolección en impresora principal completada con éxito.");
                return new CreateCashCollectionResult(true, collection, true, false, null);
            }
            catch (Exception exPrimary)
            {
                _logger.LogWarning(exPrimary, "Error al imprimir en impresora principal. Intentando con impresora de respaldo...");

                try
                {
                    // Mantener la espera de 5 segundos establecida en el flujo de negocio original
                    await Task.Delay(5000, cancellationToken);

                    // Buscar impresora de respaldo
                    var backupRegister = await _cashRegisterService.GetBackupRegisterAsync(request.CashRegisterId, cancellationToken);
                    if (backupRegister != null)
                    {
                        _logger.LogInformation("Intentando imprimir recolección en impresora de respaldo {PrinterIp}:{PrinterPort} ({BackupName})", 
                            backupRegister.PrinterIp, backupRegister.PrinterPort, backupRegister.Name);
                        
                        await _printingService.PrintCashCollectionAsync(collection, backupRegister.PrinterIp, backupRegister.PrinterPort, cancellationToken);
                        
                        _logger.LogInformation("Impresión física de recolección en impresora de respaldo completada con éxito.");
                        return new CreateCashCollectionResult(true, collection, true, true, $"Recolección registrada exitosamente e impresa en impresora de respaldo: {backupRegister.Name}.");
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró impresora de respaldo configurada para la caja {CashRegisterId}", request.CashRegisterId);
                        return new CreateCashCollectionResult(true, collection, false, false, "Recolección registrada pero falló la impresora principal y no hay impresora de respaldo.");
                    }
                }
                catch (Exception exBackup)
                {
                    _logger.LogError(exBackup, "Error de hardware al intentar imprimir en la impresora de respaldo.");
                    return new CreateCashCollectionResult(true, collection, false, false, "Recolección registrada, pero fallaron las impresoras principal y de respaldo.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al registrar la recolección de efectivo.");
            return new CreateCashCollectionResult(false, null, false, false, ex.Message);
        }
    }
}
