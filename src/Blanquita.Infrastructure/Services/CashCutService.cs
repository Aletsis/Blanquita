using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class CashCutService : ICashCutService
{
    private readonly ICashCutRepository _repository;
    private readonly ILogger<CashCutService> _logger;
    private readonly ICashCollectionService _cashCollectionService;
    private readonly ISupervisorService _supervisorService;
    private readonly ICashierService _cashierService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly IBranchService _branchService;

    public CashCutService(
        ICashCutRepository repository,
        ILogger<CashCutService> logger,
        ICashCollectionService cashCollectionService,
        ISupervisorService supervisorService,
        ICashierService cashierService,
        ICashRegisterService cashRegisterService,
        IBranchService branchService)
    {
        _repository = repository;
        _logger = logger;
        _cashCollectionService = cashCollectionService;
        _supervisorService = supervisorService;
        _cashierService = cashierService;
        _cashRegisterService = cashRegisterService;
        _branchService = branchService;
    }

    public async Task<CashCutDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCut = await _repository.GetByIdAsync(id, cancellationToken);
        return cashCut?.ToDto();
    }

    public async Task<IEnumerable<CashCutDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cashCuts = await _repository.GetAllAsync(cancellationToken);
        return cashCuts.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<CashCutDto>> SearchAsync(SearchCashCutRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        request.Validate();

        _logger.LogInformation(
            "Searching cash cuts - DateRange: {Start} to {End}, Sucursal: {Sucursal}, Register: {Register}, Cashier: {Cashier}",
            request.FechaInicio,
            request.FechaFin,
            request.Sucursal?.Nombre ?? "All",
            request.CashRegisterName ?? "All",
            request.CashierName ?? "All");

        // Obtener todos los cortes
        var allCuts = await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de fecha
        if (request.HasDateFilter())
        {
            var (inicio, fin) = request.GetNormalizedDateRange();
            allCuts = allCuts.Where(c =>
                c.CutDateTime >= inicio && c.CutDateTime <= fin);
        }

        // Aplicar filtro de sucursal
        if (request.HasSucursalFilter())
        {
            var sucursalNombre = request.Sucursal!.Nombre;
            allCuts = allCuts.Where(c =>
                c.BranchName.Equals(sucursalNombre, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de caja registradora
        if (request.HasCashRegisterFilter())
        {
            allCuts = allCuts.Where(c =>
                c.CashRegisterName.Equals(request.CashRegisterName, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de cajera
        if (request.HasCashierFilter())
        {
            allCuts = allCuts.Where(c =>
                c.CashierName.Contains(request.CashierName!, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de supervisor
        if (request.HasSupervisorFilter())
        {
            allCuts = allCuts.Where(c =>
                c.SupervisorName.Contains(request.SupervisorName!, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de monto
        if (request.HasAmountFilter())
        {
            var (min, max) = request.GetAmountRange();
            allCuts = allCuts.Where(c =>
            {
                var total = c.GetGrandTotal();
                return total >= min && total <= max;
            });
        }

        var results = allCuts.ToList();
        var totalCount = results.Count;

        // Aplicar ordenamiento
        if (request.HasSorting())
        {
            results = request.SortColumn?.ToLower() switch
            {
                "cutdatetime" or "date" => request.SortAscending
                    ? results.OrderBy(c => c.CutDateTime).ToList()
                    : results.OrderByDescending(c => c.CutDateTime).ToList(),
                "cashregistername" or "register" => request.SortAscending
                    ? results.OrderBy(c => c.CashRegisterName).ToList()
                    : results.OrderByDescending(c => c.CashRegisterName).ToList(),
                "cashiername" or "cashier" => request.SortAscending
                    ? results.OrderBy(c => c.CashierName).ToList()
                    : results.OrderByDescending(c => c.CashierName).ToList(),
                "supervisorname" or "supervisor" => request.SortAscending
                    ? results.OrderBy(c => c.SupervisorName).ToList()
                    : results.OrderByDescending(c => c.SupervisorName).ToList(),
                "grandtotal" or "total" => request.SortAscending
                    ? results.OrderBy(c => c.GetGrandTotal()).ToList()
                    : results.OrderByDescending(c => c.GetGrandTotal()).ToList(),
                _ => results.OrderByDescending(c => c.CutDateTime).ToList()
            };
        }
        else
        {
            // Ordenamiento por defecto: más reciente primero
            results = results.OrderByDescending(c => c.CutDateTime).ToList();
        }

        // Aplicar paginación si se especificó
        if (request.RequiresPagination())
        {
            results = results
                .Skip(request.GetSkip())
                .Take(request.PageSize!.Value)
                .ToList();
        }

        _logger.LogInformation(
            "Found {Count} cash cuts matching criteria (Total: {Total})",
            results.Count,
            totalCount);

        return results.Select(c => c.ToDto());
    }

    public async Task<CashCutDto> ProcessCashCutAsync(ProcessCashCutRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Get Entities and validate existence
        var register = await _cashRegisterService.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (register == null) throw new InvalidOperationException($"Register {request.CashRegisterId} not found");

        var supervisor = await _supervisorService.GetByIdAsync(request.SupervisorId, cancellationToken);
        if (supervisor == null) throw new InvalidOperationException($"Supervisor {request.SupervisorId} not found");

        var cashier = await _cashierService.GetByIdAsync(request.CashierId, cancellationToken);
        if (cashier == null) throw new InvalidOperationException($"Cashier {request.CashierId} not found");
        
        var branch = await _branchService.GetByIdAsync(supervisor.BranchId, cancellationToken);
        var branchName = branch?.Name ?? "Unknown";

        // 2. Fetch Collections
        var today = DateTime.Today;
        var endOfDay = today.AddDays(1).AddTicks(-1);

        var searchRequest = new SearchCashCollectionRequest
        {
            FechaInicio = today,
            FechaFin = endOfDay,
            CashRegisterName = register.Name,
            IsCut = false
        };

        var collections = await _cashCollectionService.SearchAsync(searchRequest, cancellationToken);
        
        var registerCollections = collections.ToList();

        // 3. Sum Totals from collections
        int totalThousands = registerCollections.Sum(r => r.Thousands);
        int totalFiveHundreds = registerCollections.Sum(r => r.FiveHundreds);
        int totalTwoHundreds = registerCollections.Sum(r => r.TwoHundreds);
        int totalHundreds = registerCollections.Sum(r => r.Hundreds);
        int totalFifties = registerCollections.Sum(r => r.Fifties);
        int totalTwenties = registerCollections.Sum(r => r.Twenties);

        // 4. Mark collections as cut
        await _cashCollectionService.MarkAsCutAsync(register.Name, cancellationToken);

        // 5. Create DTO
        var createDto = new CreateCashCutDto
        {
            TotalThousands = totalThousands,
            TotalFiveHundreds = totalFiveHundreds,
            TotalTwoHundreds = totalTwoHundreds,
            TotalHundreds = totalHundreds,
            TotalFifties = totalFifties,
            TotalTwenties = totalTwenties,
            TotalSlips = request.TotalSlips,
            TotalCards = request.TotalCards,
            CashRegisterName = register.Name,
            SupervisorName = supervisor.Name,
            CashierName = cashier.Name, 
            BranchName = branchName
        };


        var calculatedCashTotal = (totalThousands * 1000m) + (totalFiveHundreds * 500m) + (totalTwoHundreds * 200m) +
                                  (totalHundreds * 100m) + (totalFifties * 50m) + (totalTwenties * 20m);

        _logger.LogInformation(
            "Processing Cash Cut - Register: {Register}, Collections: {Count}. " +
            "Calc Cash: {CashTotal:C2}, Slips: {Slips:C2}, Cards: {Cards:C2}. " +
            "Details -> 1000x{T}, 500x{FH}, 200x{TH}, 100x{H}, 50x{F}, 20x{Tw}",
            register.Name, registerCollections.Count,
            calculatedCashTotal, request.TotalSlips, request.TotalCards,
            totalThousands, totalFiveHundreds, totalTwoHundreds, totalHundreds, totalFifties, totalTwenties);

        return await CreateAsync(createDto, cancellationToken);
    }



    public async Task<CashCutDto> CreateAsync(CreateCashCutDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var cashCut = dto.ToEntity();

            // Validate that the cash cut is valid (has a grand total > 0)
            if (!cashCut.IsValid())
            {
                _logger.LogWarning("Attempted to create invalid cash cut with zero grand total");
                throw new InvalidOperationException("Cannot create a cash cut with zero grand total");
            }

            await _repository.AddAsync(cashCut, cancellationToken);
            
            _logger.LogInformation(
                "Cash cut created successfully. Register: {CashRegister}, Total: {GrandTotal}, DateTime: {DateTime}",
                cashCut.CashRegisterName,
                cashCut.GetGrandTotal(),
                cashCut.CutDateTime);

            return cashCut.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cash cut for register {CashRegister}", dto.CashRegisterName);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCut = await _repository.GetByIdAsync(id, cancellationToken);
        if (cashCut == null)
        {
            throw new EntityNotFoundException("CashCut", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        
        _logger.LogInformation(
            "Cash cut deleted. ID: {Id}, Register: {CashRegister}, DateTime: {DateTime}",
            id,
            cashCut.CashRegisterName,
            cashCut.CutDateTime);
    }
}
