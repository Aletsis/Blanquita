using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Conciliaciones;

/// <summary>
/// Handler para el comando SaveConciliacionCorteCommand.
/// </summary>
public class SaveConciliacionCorteCommandHandler : IRequestHandler<SaveConciliacionCorteCommand, bool>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly ILogger<SaveConciliacionCorteCommandHandler> _logger;

    public SaveConciliacionCorteCommandHandler(
        IConciliacionService conciliacionService,
        ILogger<SaveConciliacionCorteCommandHandler> logger)
    {
        _conciliacionService = conciliacionService;
        _logger = logger;
    }

    public async Task<bool> Handle(SaveConciliacionCorteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Guardando conciliación para el turno {ShiftId} en la sucursal {BranchName}", request.ShiftId, request.BranchName);

        try
        {
            // Fórmulas financieras centralizadas
            var totalEfectivo = request.TotalRecolecciones + request.EfectivoEntregado;
            var totalTarjetas = request.Banregio + request.Banbajio;
            var devoluciones = Math.Abs(request.ReturnsTotal);
            var totalEntregado = totalEfectivo + totalTarjetas;
            var totalEsperado = request.TotalSold - devoluciones;
            var diferencia = totalEntregado - totalEsperado;

            var dto = new ConciliacionCorteDto
            {
                AperturaId = request.ShiftId,
                Sucursal = request.BranchName,
                Caja = request.CashRegisterName,
                Cajero = request.CashierName,
                TotalRecolecciones = request.TotalRecolecciones,
                EfectivoEntregado = request.EfectivoEntregado,
                TotalEfectivo = totalEfectivo,
                Banregio = request.Banregio,
                Banbajio = request.Banbajio,
                TotalTarjetas = totalTarjetas,
                Devoluciones = devoluciones,
                TotalEntregado = totalEntregado,
                TotalEsperado = totalEsperado,
                Diferencia = diferencia
            };

            await _conciliacionService.SaveConciliacionCorteAsync(dto, cancellationToken);
            _logger.LogInformation("Conciliación de corte para turno {ShiftId} guardada con éxito en el handler", request.ShiftId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el guardado de la conciliación para el turno {ShiftId}", request.ShiftId);
            throw;
        }
    }
}
