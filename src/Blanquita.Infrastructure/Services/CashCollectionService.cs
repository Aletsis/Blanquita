using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class CashCollectionService : ICashCollectionService
{
    private readonly ICashCollectionRepository _repository;
    private readonly ILogger<CashCollectionService> _logger;

    public CashCollectionService(ICashCollectionRepository repository, ILogger<CashCollectionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CashCollectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCollection = await _repository.GetByIdAsync(id, cancellationToken);
        return cashCollection?.ToDto();
    }

    public async Task<IEnumerable<CashCollectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cashCollections = await _repository.GetAllAsync(cancellationToken);
        return cashCollections.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<CashCollectionDto>> SearchAsync(SearchCashCollectionRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        request.Validate();

        _logger.LogInformation(
            "Searching cash collections - DateRange: {Start} to {End}, Sucursal: {Sucursal}, Register: {Register}",
            request.FechaInicio,
            request.FechaFin,
            request.Sucursal?.Nombre ?? "All",
            request.CashRegisterName ?? "All");

        // Obtener todas las recolecciones
        var allCollections = await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de fecha
        if (request.HasDateFilter())
        {
            var (inicio, fin) = request.GetNormalizedDateRange();
            allCollections = allCollections.Where(c =>
                c.CollectionDateTime >= inicio && c.CollectionDateTime <= fin);
        }

        // Aplicar filtro de sucursal (si aplica - necesitarías tener esta info en CashCollection)
        if (request.HasSucursalFilter())
        {
            // Nota: Esto requeriría que CashCollection tenga información de sucursal
            // Por ahora lo dejamos como comentario
            // var sucursalNombre = request.Sucursal!.Nombre;
            // allCollections = allCollections.Where(c => c.BranchName == sucursalNombre);
        }

        // Aplicar filtro de caja registradora
        if (request.HasCashRegisterFilter())
        {
            allCollections = allCollections.Where(c =>
                c.CashRegisterName.Equals(request.CashRegisterName, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de estado de corte
        if (request.HasCutStatusFilter())
        {
            allCollections = allCollections.Where(c => c.IsForCashCut == request.IsCut!.Value);
        }

        var results = allCollections.ToList();

        // Aplicar paginación si se especificó
        if (request.RequiresPagination())
        {
            results = results
                .Skip(request.GetSkip())
                .Take(request.PageSize!.Value)
                .ToList();
        }

        _logger.LogInformation("Found {Count} cash collections matching criteria", results.Count);

        return results.Select(c => c.ToDto());
    }

    public async Task<CashCollectionDto> CreateAsync(CreateCashCollectionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the next folio number
            var nextFolio = await _repository.GetNextFolioAsync(dto.CashRegisterName, cancellationToken);

            // Create the cash collection entity
            var cashCollection = CashCollection.Create(
                dto.Thousands,
                dto.FiveHundreds,
                dto.TwoHundreds,
                dto.Hundreds,
                dto.Fifties,
                dto.Twenties,
                dto.CashRegisterName,
                dto.CashierName,
                dto.SupervisorName,
                nextFolio,
                dto.IsForCashCut);

            await _repository.AddAsync(cashCollection, cancellationToken);

            _logger.LogInformation(
                "Cash collection created. Folio: {Folio}, Register: {CashRegister}, Amount: {Amount}, IsForCashCut: {IsForCashCut}",
                nextFolio,
                dto.CashRegisterName,
                cashCollection.GetTotalAmount(),
                dto.IsForCashCut);

            return cashCollection.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cash collection for register {CashRegister}", dto.CashRegisterName);
            throw;
        }
    }

    public async Task MarkAsCutAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        var pendingCollections = await _repository.GetPendingCollectionsByRegisterAsync(cashRegisterName, cancellationToken);

        foreach (var collection in pendingCollections)
        {
            collection.MarkAsForCashCut();
            await _repository.UpdateAsync(collection, cancellationToken);
        }

        _logger.LogInformation("Marked {Count} collections as cut for register {Register}", pendingCollections.Count(), cashRegisterName);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCollection = await _repository.GetByIdAsync(id, cancellationToken);
        if (cashCollection == null)
        {
            throw new EntityNotFoundException("CashCollection", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation(
            "Cash collection deleted. ID: {Id}, Folio: {Folio}, Register: {CashRegister}",
            id,
            cashCollection.Folio,
            cashCollection.CashRegisterName);
    }
}
