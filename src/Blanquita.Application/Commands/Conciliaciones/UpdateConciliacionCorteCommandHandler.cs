using System;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Conciliaciones;

public class UpdateConciliacionCorteCommandHandler : IRequestHandler<UpdateConciliacionCorteCommand, bool>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly IConciliacionCorteRepository _conciliacionCorteRepository;
    private readonly ILogger<UpdateConciliacionCorteCommandHandler> _logger;

    public UpdateConciliacionCorteCommandHandler(
        IConciliacionService conciliacionService,
        IConciliacionCorteRepository conciliacionCorteRepository,
        ILogger<UpdateConciliacionCorteCommandHandler> logger)
    {
        _conciliacionService = conciliacionService;
        _conciliacionCorteRepository = conciliacionCorteRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateConciliacionCorteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando actualización de conciliación de corte con ID: {Id}", request.Id);

        var existing = await _conciliacionCorteRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null)
        {
            throw new InvalidOperationException($"No se encontró la conciliación de corte con ID {request.Id}.");
        }

        // Aviso temprano; la base de datos vuelve a validar la versión al guardar
        if (existing.Version != request.Version)
        {
            throw new ConcurrencyConflictException(
                "Otro usuario modificó esta conciliación mientras la editaba. Vuelva a abrirla para ver los cambios.");
        }

        // El total de salidas se calcula de la lista que se guarda, no del monto enviado por la pantalla
        var salidasEfectivo = request.Salidas?.Sum(s => s.Monto) ?? request.SalidasEfectivo;

        if (TieneNuevasSalidas(existing, request.Salidas, salidasEfectivo) &&
            !await _conciliacionService.PermiteSalidasEfectivoAsync(existing.Caja, cancellationToken))
        {
            throw new BusinessRuleViolationException(
                $"La caja '{existing.Caja}' o su sucursal no tienen habilitado el registro de salidas de efectivo.");
        }

        var totalEfectivo = existing.TotalRecolecciones + request.EfectivoEntregado + salidasEfectivo;
        var totalTarjetas = request.Banregio + request.Banbajio;
        var totalEntregado = totalEfectivo + totalTarjetas;
        var diferencia = totalEntregado - existing.TotalEsperado;

        var dto = new ConciliacionCorteDto
        {
            Id = request.Id,
            AperturaId = existing.AperturaId,
            Sucursal = existing.Sucursal,
            Caja = existing.Caja,
            Cajero = existing.Cajero,
            TotalRecolecciones = existing.TotalRecolecciones,
            EfectivoEntregado = request.EfectivoEntregado,
            SalidasEfectivo = salidasEfectivo,
            TotalEfectivo = totalEfectivo,
            Banregio = request.Banregio,
            Banbajio = request.Banbajio,
            TotalTarjetas = totalTarjetas,
            Devoluciones = existing.Devoluciones,
            TotalEntregado = totalEntregado,
            TotalEsperado = existing.TotalEsperado,
            Diferencia = diferencia,
            ModificadoPor = request.ModificadoPor,
            Version = request.Version,
            Salidas = request.Salidas ?? new(),
            Terminales = request.Terminales ?? new()
        };

        await _conciliacionService.UpdateConciliacionCorteAsync(dto, cancellationToken);
        _logger.LogInformation("Conciliación de corte ID {Id} actualizada exitosamente por {User}.", request.Id, request.ModificadoPor);

        return true;
    }

    private static bool TieneNuevasSalidas(
        Domain.Entities.ConciliacionCorte existing,
        List<ConciliacionSalidaEfectivoDto>? requestedSalidas,
        decimal requestedTotal)
    {
        if (requestedTotal <= 0 && (requestedSalidas == null || requestedSalidas.Count == 0))
            return false;

        if (requestedSalidas == null || requestedSalidas.Count == 0)
            return requestedTotal > existing.SalidasEfectivo;

        var existingById = existing.Salidas.ToDictionary(s => s.Id);
        return requestedSalidas.Any(s =>
            s.Id <= 0 ||
            !existingById.TryGetValue(s.Id, out var prev) ||
            prev.Monto != s.Monto ||
            prev.Motivo != s.Motivo ||
            prev.QuienAutoriza != s.QuienAutoriza);
    }
}
