using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Conciliaciones;

/// <summary>
/// Handler para el comando SaveConciliacionCorteCommand.
/// </summary>
public class SaveConciliacionCorteCommandHandler : IRequestHandler<SaveConciliacionCorteCommand, bool>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly IFoxProShiftRepository _shiftRepository;
    private readonly ILogger<SaveConciliacionCorteCommandHandler> _logger;
    private readonly IBackgroundJobQueue _backgroundJobQueue;

    public SaveConciliacionCorteCommandHandler(
        IConciliacionService conciliacionService,
        IFoxProShiftRepository shiftRepository,
        ILogger<SaveConciliacionCorteCommandHandler> logger,
        IBackgroundJobQueue backgroundJobQueue)
    {
        _conciliacionService = conciliacionService;
        _shiftRepository = shiftRepository;
        _logger = logger;
        _backgroundJobQueue = backgroundJobQueue;
    }

    public async Task<bool> Handle(SaveConciliacionCorteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Guardando conciliación para el turno {ShiftId} en la sucursal {BranchName}", request.ShiftId, request.BranchName);

        try
        {
            // Validar que el turno esté cerrado en CONTPAQi POS (Status == 1)
            var shift = await _shiftRepository.GetShiftDataAsync(request.ShiftId, cancellationToken);
            if (shift == null)
            {
                throw new InvalidOperationException($"El turno con ID {request.ShiftId} no existe en CONTPAQi POS.");
            }

            if (shift.Status != 1)
            {
                throw new InvalidOperationException($"No se puede guardar la conciliación de corte para el turno {request.ShiftId} porque no ha sido cerrado en CONTPAQi POS.");
            }

            // Ventas, devoluciones y recolecciones se vuelven a leer en el servidor; los de la pantalla solo se comparan.
            // Si cambiaron desde la consulta, el usuario cuadró contra datos viejos y debe volver a consultar.
            var totalRecolecciones = await _conciliacionService.GetTotalRecolectadoAsync(request.CashRegisterName, shift, cancellationToken);
            var totalSold = shift.TotalSold;
            var devoluciones = Math.Abs(shift.ReturnsTotal);
            var salidasEfectivo = request.Salidas?.Sum(s => s.Monto) ?? request.SalidasEfectivo;

            if (!SameAmount(totalRecolecciones, request.TotalRecolecciones) ||
                !SameAmount(totalSold, request.TotalSold) ||
                !SameAmount(devoluciones, Math.Abs(request.ReturnsTotal)) ||
                !SameAmount(salidasEfectivo, request.SalidasEfectivo))
            {
                _logger.LogWarning(
                    "Montos del turno {ShiftId} distintos a los de la pantalla. Recolecciones {Recolecciones}/{RecoPantalla}, vendido {Vendido}/{VendidoPantalla}, devoluciones {Devoluciones}/{DevPantalla}, salidas {Salidas}/{SalidasPantalla}",
                    request.ShiftId, totalRecolecciones, request.TotalRecolecciones, totalSold, request.TotalSold,
                    devoluciones, Math.Abs(request.ReturnsTotal), salidasEfectivo, request.SalidasEfectivo);

                throw new InvalidOperationException(
                    "Los montos del turno cambiaron desde que se consultaron (ventas, devoluciones o recolecciones). Vuelva a consultar el turno y cuadre de nuevo antes de guardar.");
            }

            // Fórmulas financieras centralizadas
            var totalEfectivo = totalRecolecciones + request.EfectivoEntregado + salidasEfectivo;
            var totalTarjetas = request.Banregio + request.Banbajio;
            var totalEntregado = totalEfectivo + totalTarjetas;
            var totalEsperado = totalSold - devoluciones;
            var diferencia = totalEntregado - totalEsperado;

            var dto = new ConciliacionCorteDto
            {
                AperturaId = request.ShiftId,
                Sucursal = request.BranchName,
                Caja = request.CashRegisterName,
                Cajero = request.CashierName,
                TotalRecolecciones = totalRecolecciones,
                EfectivoEntregado = request.EfectivoEntregado,
                SalidasEfectivo = salidasEfectivo,
                TotalEfectivo = totalEfectivo,
                Banregio = request.Banregio,
                Banbajio = request.Banbajio,
                TotalTarjetas = totalTarjetas,
                Devoluciones = devoluciones,
                TotalEntregado = totalEntregado,
                TotalEsperado = totalEsperado,
                Diferencia = diferencia,
                Fecha = request.Fecha,
                Usuario = request.Usuario,
                Salidas = request.Salidas ?? new(),
                Terminales = request.Terminales ?? new()
            };

            await _conciliacionService.SaveConciliacionCorteAsync(dto, cancellationToken);
            _logger.LogInformation("Conciliación de corte para turno {ShiftId} guardada con éxito en el handler", request.ShiftId);

            // Alertas por Descuadres Financieros (Fase 5 - 5.2).
            // Se encolan en Hangfire: el correo y el WhatsApp no deben retrasar ni hacer fallar el guardado
            if (Math.Abs(diferencia) >= DescuadreAlertService.Threshold)
            {
                _logger.LogWarning("Se detectó un descuadre financiero de {Diferencia:C} (umbral >= $50.00 MXN) para el turno {ShiftId}. Encolando alertas...", diferencia, request.ShiftId);

                try
                {
                    _backgroundJobQueue.EnqueueDescuadreAlert(new DescuadreAlertDto
                    {
                        ShiftId = request.ShiftId,
                        BranchName = request.BranchName,
                        CashRegisterName = request.CashRegisterName,
                        CashierName = request.CashierName,
                        TotalEntregado = totalEntregado,
                        TotalEsperado = totalEsperado,
                        Diferencia = diferencia
                    });
                }
                catch (Exception ex)
                {
                    // La conciliación ya quedó guardada; solo se pierde el aviso
                    _logger.LogError(ex, "No se pudo encolar la alerta de descuadre del turno {ShiftId}", request.ShiftId);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el guardado de la conciliación para el turno {ShiftId}", request.ShiftId);
            throw;
        }
    }

    // La pantalla redondea a centavos; diferencias menores no cuentan como cambio
    private static bool SameAmount(decimal server, decimal screen) =>
        Math.Round(server, 2, MidpointRounding.AwayFromZero) == Math.Round(screen, 2, MidpointRounding.AwayFromZero);
}
