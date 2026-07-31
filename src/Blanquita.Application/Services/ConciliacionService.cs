using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

public class ConciliacionService : IConciliacionService
{
    private readonly IFoxProShiftRepository _foxProShiftRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ICashierRepository _cashierRepository;
    private readonly ICashCollectionRepository _cashCollectionRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IConciliacionCorteRepository _conciliacionCorteRepository;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<ConciliacionService> _logger;

    public ConciliacionService(
        IFoxProShiftRepository foxProShiftRepository,
        ICashRegisterRepository cashRegisterRepository,
        ICashierRepository cashierRepository,
        ICashCollectionRepository cashCollectionRepository,
        IBranchRepository branchRepository,
        IConciliacionCorteRepository conciliacionCorteRepository,
        IConfiguracionService configuracionService,
        ILogger<ConciliacionService> logger)
    {
        _foxProShiftRepository = foxProShiftRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _cashierRepository = cashierRepository;
        _cashCollectionRepository = cashCollectionRepository;
        _branchRepository = branchRepository;
        _conciliacionCorteRepository = conciliacionCorteRepository;
        _configuracionService = configuracionService;
        _logger = logger;
    }

    public async Task<ConciliacionResultDto> GetConciliacionAsync(int cashRegisterId, int shiftId, int cashierId, DateTime date, CancellationToken cancellationToken = default)
    {
        var cashRegister = await _cashRegisterRepository.GetByIdAsync(cashRegisterId, cancellationToken);
        if (cashRegister == null) throw new Exception("Caja no encontrada");

        // Obtener sucursal de la caja
        var branch = await _branchRepository.GetByIdAsync(cashRegister.BranchId.Value, cancellationToken);
        var branchName = branch?.Name ?? "Desconocida";

        // 1. Obtener datos de FoxPro usando el shiftId específico
        var shiftData = await _foxProShiftRepository.GetShiftDataAsync(shiftId, cancellationToken);
        
        var currentCashierId = cashierId > 0 ? cashierId : (shiftData?.CashierId ?? 0);
        var cashier = currentCashierId > 0 
            ? await _cashierRepository.GetByIDContpaqAsync(currentCashierId, cancellationToken)
            : null;

        // 2. Obtener recolecciones internas (solo las que ocurrieron durante el horario de este turno)
        var startTime = shiftData?.OpeningTime ?? date.Date;
        var endTime = shiftData?.ClosingTime ?? DateTime.Now;

        var collections = await _cashCollectionRepository.GetCollectionsByRegisterAndTimeAsync(
            cashRegister.Name, 
            startTime, 
            endTime, 
            cancellationToken);
        
        var totalRecolectado = collections
            .Sum(c => c.GetTotalAmount().Amount);

        return new ConciliacionResultDto
        {
            BranchName = branchName,
            CashRegisterName = cashRegister.Name,
            CashierName = cashier?.Name ?? $"Cajero ID: {currentCashierId}",
            CashCollected = shiftData?.CashCollected ?? 0,
            CardCollected = shiftData?.CardCollected ?? 0,
            ReturnsTotal = shiftData?.ReturnsTotal ?? 0,
            TotalRecolectado = totalRecolectado,
            Status = shiftData?.Status ?? 0,
            Fecha = shiftData?.OpeningTime ?? date
        };
    }

    public async Task SaveConciliacionCorteAsync(ConciliacionCorteDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Guardando conciliación de corte para Sucursal: {Sucursal}, Caja: {Caja}, Cajero: {Cajero}", 
            dto.Sucursal, dto.Caja, dto.Cajero);

        var conciliacion = new Domain.Entities.ConciliacionCorte(
            dto.AperturaId,
            dto.Sucursal,
            dto.Caja,
            dto.Cajero,
            dto.TotalRecolecciones,
            dto.EfectivoEntregado,
            dto.TotalEfectivo,
            dto.Banregio,
            dto.Banbajio,
            dto.TotalTarjetas,
            dto.Devoluciones,
            dto.TotalEntregado,
            dto.TotalEsperado,
            dto.Diferencia,
            dto.Fecha
        );

        await _conciliacionCorteRepository.AddAsync(conciliacion, cancellationToken);
        _logger.LogInformation("Conciliación de corte guardada exitosamente en la base de datos.");
    }

    public async Task<IEnumerable<ConciliacionCorteDto>> GetConciliacionesByBranchAndDateAsync(string branchName, DateTime date, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo conciliaciones para la sucursal: {BranchName} en la fecha {Date}", branchName, date);
        var entities = await _conciliacionCorteRepository.GetByBranchAndDateAsync(branchName, date, cancellationToken);
        
        return entities.Select(c => new ConciliacionCorteDto
        {
            AperturaId = c.AperturaId,
            Sucursal = c.Sucursal,
            Caja = c.Caja,
            Cajero = c.Cajero,
            TotalRecolecciones = c.TotalRecolecciones,
            EfectivoEntregado = c.EfectivoEntregado,
            TotalEfectivo = c.TotalEfectivo,
            Banregio = c.Banregio,
            Banbajio = c.Banbajio,
            TotalTarjetas = c.TotalTarjetas,
            Devoluciones = c.Devoluciones,
            TotalEntregado = c.TotalEntregado,
            TotalEsperado = c.TotalEsperado,
            Diferencia = c.Diferencia,
            Fecha = c.Fecha,
            FechaCreacion = c.FechaCreacion
        }).ToList();
    }

    public async Task<IEnumerable<AvailableBoxDto>> GetAvailableBoxesAsync(DateTime date, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try 
        {
            _logger.LogInformation("Iniciando GetAvailableBoxesAsync para la fecha {Date} y Sucursal {BranchId}", date, branchId);

            // 1. Obtener todos los turnos del archivo FoxPro para esa fecha
            var shifts = await _foxProShiftRepository.GetTodayShiftsAsync(date, cancellationToken);
            _logger.LogInformation("Se encontraron {Count} turnos en FoxPro", shifts.Count());
            
            // 2. Obtener las cajas configuradas internamente (filtrando por sucursal si corresponde)
            int? targetBranchId = branchId;
            if (!targetBranchId.HasValue || targetBranchId.Value <= 0)
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                var path = config.Pos10042Path;
                if (!string.IsNullOrEmpty(path))
                {
                    var branches = await _branchRepository.GetAllAsync(cancellationToken);
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

            IEnumerable<Domain.Entities.CashRegister> allBoxes;
            if (targetBranchId.HasValue && targetBranchId.Value > 0)
            {
                allBoxes = await _cashRegisterRepository.GetByBranchAsync(targetBranchId.Value, cancellationToken);
            }
            else
            {
                allBoxes = await _cashRegisterRepository.GetAllAsync(cancellationToken);
            }
            _logger.LogInformation("Se encontraron {Count} cajas configuradas", allBoxes.Count());

            // Obtener IDs de apertura ya conciliados para esta fecha para filtrarlos
            var conciliatedIds = await _conciliacionCorteRepository.GetAlreadyConciliatedShiftIdsAsync(date, cancellationToken);
            var conciliatedSet = conciliatedIds.ToHashSet();

            // Agrupar cajas por IdContpaqi para evitar errores de clave duplicada si la configuración está repetida
            var boxDict = allBoxes
                .GroupBy(b => b.IdContpaqi)
                .ToDictionary(g => g.Key, g => g.First());

            // 3. Crear lista de disponibles mapeando cada turno a su caja
            var result = new List<AvailableBoxDto>();
            foreach (var shift in shifts)
            {
                // Si el turno ya fue conciliado, no lo mostramos en la lista
                if (conciliatedSet.Contains(shift.InternalId))
                {
                    _logger.LogInformation("Turno {ShiftId} ya conciliado, omitiendo.", shift.InternalId);
                    continue;
                }

                if (boxDict.TryGetValue(shift.IdContpaqi, out var box))
                {
                    result.Add(new AvailableBoxDto
                    {
                        Id = box.Id,
                        ShiftId = shift.InternalId,
                        Name = box.Name,
                        IdContpaqi = box.IdContpaqi,
                        CashierId = shift.CashierId,
                        Time = shift.OpeningTime,
                        TimeClose = shift.ClosingTime,
                        Status = shift.Status
                    });
                }
                else 
                {
                    _logger.LogWarning("Turno encontrado en FoxPro para Caja ID {IdContpaqi} pero no existe en la configuración interna", shift.IdContpaqi);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAvailableBoxesAsync");
            throw; // Re-throw to be caught by the UI
        }
    }
}
