using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

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

        // Construir query con filtros
        var results = await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de fecha
        if (request.HasDateFilter())
        {
            var (inicio, fin) = request.GetNormalizedDateRange();
            results = results.Where(c => c.CutDateTime >= inicio && c.CutDateTime <= fin);
        }

        // Aplicar filtro de sucursal
        if (request.HasSucursalFilter())
        {
            var sucursalNombre = request.Sucursal!.Nombre;
            results = results.Where(c => c.BranchName == sucursalNombre);
        }

        // Aplicar filtro de caja registradora
        if (request.HasCashRegisterFilter())
        {
            results = results.Where(c => c.CashRegisterName == request.CashRegisterName);
        }

        // Aplicar filtro de cajera
        if (request.HasCashierFilter())
        {
            results = results.Where(c => c.CashierName.Contains(request.CashierName!, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de supervisor
        if (request.HasSupervisorFilter())
        {
            results = results.Where(c => c.SupervisorName.Contains(request.SupervisorName!, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de monto
        if (request.HasAmountFilter())
        {
            var (min, max) = request.GetAmountRange();
            results = results.Where(c =>
            {
                var total = c.GetGrandTotal();
                return total >= min && total <= max;
            });
        }

        // Ordenamiento
        results = ApplySorting(results, request);

        var finalResults = results.ToList();
        var totalCount = finalResults.Count;

        // Aplicar paginación
        if (request.RequiresPagination())
        {
            finalResults = finalResults
                .Skip(request.GetSkip())
                .Take(request.PageSize!.Value)
                .ToList();
        }

        _logger.LogInformation("Found {Count} cash cuts matching criteria", totalCount);

        return finalResults.Select(c => c.ToDto());
    }

    private IEnumerable<CashCut> ApplySorting(IEnumerable<CashCut> query, SearchCashCutRequest request)
    {
        if (!request.HasSorting())
        {
            return query.OrderByDescending(c => c.CutDateTime);
        }

        return request.SortColumn?.ToLower() switch
        {
            "cutdatetime" or "date" => request.SortAscending
                ? query.OrderBy(c => c.CutDateTime)
                : query.OrderByDescending(c => c.CutDateTime),
            "cashregistername" or "register" => request.SortAscending
                ? query.OrderBy(c => c.CashRegisterName)
                : query.OrderByDescending(c => c.CashRegisterName),
            "cashiername" or "cashier" => request.SortAscending
                ? query.OrderBy(c => c.CashierName)
                : query.OrderByDescending(c => c.CashierName),
            "supervisorname" or "supervisor" => request.SortAscending
                ? query.OrderBy(c => c.SupervisorName)
                : query.OrderByDescending(c => c.SupervisorName),
            "branchname" or "branch" => request.SortAscending
                ? query.OrderBy(c => c.BranchName)
                : query.OrderByDescending(c => c.BranchName),
            "grandtotal" or "total" => request.SortAscending
                ? query.OrderBy(c => (decimal)c.GetGrandTotal())
                : query.OrderByDescending(c => (decimal)c.GetGrandTotal()),
            _ => query.OrderByDescending(c => c.CutDateTime)
        };
    }

    public async Task<CashCutDto> ProcessCashCutAsync(ProcessCashCutRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Get Entities and validate existence
        var register = await _cashRegisterService.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (register == null) throw new InvalidOperationException($"Register {request.CashRegisterId} not found");

        string supervisorName = request.SupervisorName ?? string.Empty;
        int branchId = request.BranchId ?? 0;

        if (request.SupervisorId > 0)
        {
            var supervisor = await _supervisorService.GetByIdAsync(request.SupervisorId, cancellationToken);
            if (supervisor != null)
            {
                supervisorName = supervisor.Name;
                if (branchId <= 0) branchId = supervisor.BranchId;
            }
        }

        if (string.IsNullOrWhiteSpace(supervisorName))
        {
            supervisorName = "Usuario";
        }

        if (branchId <= 0)
        {
            branchId = register.BranchId;
        }

        var cashier = await _cashierService.GetByIdAsync(request.CashierId, cancellationToken);
        if (cashier == null) throw new InvalidOperationException($"Cashier {request.CashierId} not found");
        
        var branch = await _branchService.GetByIdAsync(branchId, cancellationToken);
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
            TotalBanbajio = request.TotalBanbajio,
            TotalBanregio = request.TotalBanregio,
            CashRegisterName = register.Name,
            SupervisorName = supervisorName,
            CashierName = cashier.Name, 
            BranchName = branchName
        };

        return await CreateAsync(createDto, cancellationToken);
    }

    public async Task<CashCutDto> CreateAsync(CreateCashCutDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var cashCut = dto.ToEntity();

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
