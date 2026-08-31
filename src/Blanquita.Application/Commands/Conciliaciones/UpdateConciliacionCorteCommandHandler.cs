using System;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
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

        var totalEfectivo = existing.TotalRecolecciones + request.EfectivoEntregado + request.SalidasEfectivo;
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
            SalidasEfectivo = request.SalidasEfectivo,
            TotalEfectivo = totalEfectivo,
            Banregio = request.Banregio,
            Banbajio = request.Banbajio,
            TotalTarjetas = totalTarjetas,
            Devoluciones = existing.Devoluciones,
            TotalEntregado = totalEntregado,
            TotalEsperado = existing.TotalEsperado,
            Diferencia = diferencia,
            ModificadoPor = request.ModificadoPor,
            Salidas = request.Salidas ?? new(),
            Terminales = request.Terminales ?? new()
        };

        await _conciliacionService.UpdateConciliacionCorteAsync(dto, cancellationToken);
        _logger.LogInformation("Conciliación de corte ID {Id} actualizada exitosamente por {User}.", request.Id, request.ModificadoPor);

        return true;
    }
}
