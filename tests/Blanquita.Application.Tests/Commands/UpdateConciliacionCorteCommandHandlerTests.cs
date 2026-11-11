using System.Reflection;
using Blanquita.Application.Commands.Conciliaciones;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Commands;

public class UpdateConciliacionCorteCommandHandlerTests
{
    private readonly Mock<IConciliacionService> _mockConciliacionService = new();
    private readonly Mock<IConciliacionCorteRepository> _mockRepository = new();
    private readonly UpdateConciliacionCorteCommandHandler _handler;

    public UpdateConciliacionCorteCommandHandlerTests()
    {
        _handler = new UpdateConciliacionCorteCommandHandler(
            _mockConciliacionService.Object,
            _mockRepository.Object,
            new Mock<ILogger<UpdateConciliacionCorteCommandHandler>>().Object);
    }

    private ConciliacionCorte SetupExisting(uint version)
    {
        var existing = new ConciliacionCorte(
            aperturaId: 123, sucursal: "Sucursal", caja: "Caja 1", cajero: "Maria",
            totalRecolecciones: 1500m, efectivoEntregado: 500m, salidasEfectivo: 0m, totalEfectivo: 2000m,
            banregio: 300m, banbajio: 200m, totalTarjetas: 500m, devoluciones: 100m,
            totalEntregado: 2500m, totalEsperado: 2500m, diferencia: 0m, fecha: DateTime.UtcNow);

        typeof(ConciliacionCorte).GetProperty(nameof(ConciliacionCorte.Version))!
            .SetValue(existing, version);

        _mockRepository.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        return existing;
    }

    [Fact]
    public async Task Handle_VersionChangedSinceLoad_ThrowsConflictWithoutSaving()
    {
        SetupExisting(version: 42);

        var command = new UpdateConciliacionCorteCommand(Id: 7, Version: 41,
            EfectivoEntregado: 450m, SalidasEfectivo: 0m, Banregio: 300m, Banbajio: 200m);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => _handler.Handle(command, CancellationToken.None));

        _mockConciliacionService.Verify(s => s.UpdateConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameVersion_RecalculatesTotalsFromStoredValuesAndSalidasList()
    {
        SetupExisting(version: 42);
        _mockConciliacionService
            .Setup(s => s.PermiteSalidasEfectivoAsync("Caja 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var salidas = new List<ConciliacionSalidaEfectivoDto>
        {
            new() { Monto = 30m, Motivo = "Gas" },
            new() { Monto = 20m, Motivo = "Agua" }
        };

        // El monto de salidas enviado (999) se ignora: manda la lista
        var command = new UpdateConciliacionCorteCommand(Id: 7, Version: 42,
            EfectivoEntregado: 450m, SalidasEfectivo: 999m, Banregio: 300m, Banbajio: 200m,
            ModificadoPor: "admin", Salidas: salidas);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        // totalEfectivo = 1500 (guardado) + 450 + 50 = 2000; entregado = 2000 + 500 = 2500; esperado guardado = 2500
        _mockConciliacionService.Verify(s => s.UpdateConciliacionCorteAsync(It.Is<ConciliacionCorteDto>(dto =>
            dto.Version == 42 &&
            dto.SalidasEfectivo == 50m &&
            dto.TotalEfectivo == 2000m &&
            dto.TotalEntregado == 2500m &&
            dto.Diferencia == 0m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewSalidasWhenNotPermitted_ThrowsBusinessRuleViolationException()
    {
        SetupExisting(version: 42);
        _mockConciliacionService
            .Setup(s => s.PermiteSalidasEfectivoAsync("Caja 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var salidas = new List<ConciliacionSalidaEfectivoDto>
        {
            new() { Monto = 30m, Motivo = "Gas" }
        };

        var command = new UpdateConciliacionCorteCommand(Id: 7, Version: 42,
            EfectivoEntregado: 470m, SalidasEfectivo: 30m, Banregio: 300m, Banbajio: 200m,
            ModificadoPor: "admin", Salidas: salidas);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => _handler.Handle(command, CancellationToken.None));

        _mockConciliacionService.Verify(s => s.UpdateConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovingSalidasWhenNotPermitted_AllowsUpdate()
    {
        var existing = SetupExisting(version: 42);
        var salidaExistente = new ConciliacionSalidaEfectivo(50m, "Gas", "Admin", "admin");
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(salidaExistente, 10);
        existing.Salidas.Add(salidaExistente);

        _mockConciliacionService
            .Setup(s => s.PermiteSalidasEfectivoAsync("Caja 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateConciliacionCorteCommand(Id: 7, Version: 42,
            EfectivoEntregado: 500m, SalidasEfectivo: 0m, Banregio: 300m, Banbajio: 200m,
            ModificadoPor: "admin", Salidas: new List<ConciliacionSalidaEfectivoDto>());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _mockConciliacionService.Verify(s => s.UpdateConciliacionCorteAsync(It.IsAny<ConciliacionCorteDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
