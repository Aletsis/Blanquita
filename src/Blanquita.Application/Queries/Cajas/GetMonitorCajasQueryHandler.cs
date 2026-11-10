using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.Cajas;

/// <summary>
/// Handler para el query GetMonitorCajasQuery.
/// </summary>
public class GetMonitorCajasQueryHandler : IRequestHandler<GetMonitorCajasQuery, IEnumerable<CajaMonitorDto>>
{
    private readonly IFoxProShiftRepository _foxProShiftRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICashierRepository _cashierRepository;
    private readonly ICashCollectionRepository _cashCollectionRepository;
    private readonly IConciliacionCorteRepository _conciliacionCorteRepository;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<GetMonitorCajasQueryHandler> _logger;

    public GetMonitorCajasQueryHandler(
        IFoxProShiftRepository foxProShiftRepository,
        ICashRegisterRepository cashRegisterRepository,
        IBranchRepository branchRepository,
        ICashierRepository cashierRepository,
        ICashCollectionRepository cashCollectionRepository,
        IConciliacionCorteRepository conciliacionCorteRepository,
        IConfiguracionService configuracionService,
        ILogger<GetMonitorCajasQueryHandler> logger)
    {
        _foxProShiftRepository = foxProShiftRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _branchRepository = branchRepository;
        _cashierRepository = cashierRepository;
        _cashCollectionRepository = cashCollectionRepository;
        _conciliacionCorteRepository = conciliacionCorteRepository;
        _configuracionService = configuracionService;
        _logger = logger;
    }

    public async Task<IEnumerable<CajaMonitorDto>> Handle(GetMonitorCajasQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando monitor de cajas para fecha {Date} y sucursal {BranchId}", request.Date, request.BranchId);

        try
        {
            // 1. Obtener los turnos desde FoxPro/DBF para la fecha especificada
            var shifts = (await _foxProShiftRepository.GetTodayShiftsAsync(request.Date, cancellationToken)).ToList();
            if (!shifts.Any())
            {
                _logger.LogInformation("No se encontraron turnos en FoxPro para la fecha {Date}", request.Date);
                return Enumerable.Empty<CajaMonitorDto>();
            }

            // 2. Determinar sucursal objetivo si no fue especificada directamente
            int? targetBranchId = request.BranchId;
            var branches = (await _branchRepository.GetAllAsync(cancellationToken)).ToList();
            var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

            if (!targetBranchId.HasValue || targetBranchId.Value <= 0)
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                var path = config.Pos10042Path;
                if (!string.IsNullOrEmpty(path))
                {
                    string pathLower = path.ToLowerInvariant();
                    var matchedBranch = branches.FirstOrDefault(b =>
                        (!string.IsNullOrEmpty(b.Code) && pathLower.Contains(b.Code.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(b.Name) && pathLower.Contains(b.Name.Replace(" ", "").ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(b.Name) && pathLower.Contains(b.Name.Replace(" ", "_").ToLowerInvariant()))
                    );
                    if (matchedBranch != null)
                    {
                        targetBranchId = matchedBranch.Id;
                        _logger.LogInformation("Sucursal detectada automáticamente a partir del path DBF: {BranchName} (ID: {BranchId})", matchedBranch.Name, matchedBranch.Id);
                    }
                }
            }

            // 3. Obtener cajas registradoras configuradas
            IEnumerable<Domain.Entities.CashRegister> allBoxes;
            if (targetBranchId.HasValue && targetBranchId.Value > 0)
            {
                allBoxes = await _cashRegisterRepository.GetByBranchAsync(targetBranchId.Value, cancellationToken);
            }
            else
            {
                allBoxes = await _cashRegisterRepository.GetAllAsync(cancellationToken);
            }

            var boxDict = allBoxes
                .GroupBy(b => b.IdContpaqi)
                .ToDictionary(g => g.Key, g => g.First());

            // 4. Obtener turnos ya conciliados en la fecha
            var conciliatedIds = (await _conciliacionCorteRepository.GetAlreadyConciliatedShiftIdsAsync(request.Date, cancellationToken)).ToHashSet();

            // 5. Pre-cargar cajeras para mapeo rápido
            var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
            var cashierDict = cashiers
                .Where(c => c.IDContpaq > 0)
                .GroupBy(c => c.IDContpaq)
                .ToDictionary(g => g.Key, g => g.First().Name);

            // 6. Procesar cada turno y construir el DTO de monitoreo
            var result = new List<CajaMonitorDto>();

            foreach (var shift in shifts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Domain.Entities.CashRegister? box = null;
                boxDict.TryGetValue(shift.IdContpaqi, out box);

                // Si se solicitó filtrar por sucursal específica y la caja no pertenece a esa sucursal, omitir
                if (targetBranchId.HasValue && targetBranchId.Value > 0)
                {
                    if (box == null || box.BranchId.Value != targetBranchId.Value)
                    {
                        continue;
                    }
                }

                var boxName = box?.Name ?? $"Caja Contpaqi #{shift.IdContpaqi}";
                var boxId = box?.Id ?? 0;
                var branchId = box?.BranchId.Value ?? (targetBranchId ?? 0);
                string branchName = branchDict.TryGetValue(branchId, out var bName) ? bName : "Desconocida";

                // Resolver nombre de cajera
                string cashierName;
                if (cashierDict.TryGetValue(shift.CashierId, out var resolvedName))
                {
                    cashierName = resolvedName;
                }
                else
                {
                    cashierName = shift.CashierId > 0 ? $"Cajera #{shift.CashierId}" : "Sin asignar";
                }

                // Obtener recolecciones durante el turno
                var collections = await _cashCollectionRepository.GetCollectionsByRegisterAndTimeAsync(
                    boxName,
                    shift.OpeningTime,
                    shift.ClosingTime ?? DateTime.Now,
                    cancellationToken);

                var collectionDtos = collections
                    .Select(c => c.ToDto())
                    .OrderByDescending(c => c.CollectionDateTime)
                    .ToList();

                var totalRecolectado = collectionDtos.Sum(c => c.TotalAmount);

                // Verificar si está conciliada y consultar salidas si existen
                bool isConciliated = conciliatedIds.Contains(shift.InternalId);
                decimal salidasEfectivo = 0;
                int? concId = null;

                if (isConciliated)
                {
                    var conc = await _conciliacionCorteRepository.GetByAperturaIdAsync(shift.InternalId, cancellationToken);
                    if (conc != null)
                    {
                        salidasEfectivo = conc.SalidasEfectivo;
                        concId = conc.Id;
                    }
                }

                result.Add(new CajaMonitorDto
                {
                    CashRegisterId = boxId,
                    CashRegisterName = boxName,
                    IdContpaqi = shift.IdContpaqi,
                    BranchId = branchId,
                    BranchName = branchName,
                    ShiftId = shift.InternalId,
                    CashierId = shift.CashierId,
                    CashierName = cashierName,
                    OpeningTime = shift.OpeningTime,
                    ClosingTime = shift.ClosingTime,
                    Status = shift.Status,
                    IsConciliated = isConciliated,
                    ConciliacionId = concId,
                    CashCollected = shift.CashCollected,
                    CardCollected = shift.CardCollected,
                    ReturnsTotal = shift.ReturnsTotal,
                    TotalRecolectado = totalRecolectado,
                    Recolecciones = collectionDtos,
                    SalidasEfectivo = salidasEfectivo
                });
            }

            // Ordenar: primero las abiertas (Status == 0), luego por hora de apertura descendente
            return result
                .OrderBy(c => c.Status) // 0 (Abiertas) antes que 1 (Cerradas)
                .ThenByDescending(c => c.OpeningTime)
                .ThenBy(c => c.CashRegisterName)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar monitor de cajas para fecha {Date}", request.Date);
            throw;
        }
    }
}
