using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

public class CashCollectionService : ICashCollectionService
{
    private readonly ICashCollectionRepository _repository;
    private readonly ILogger<CashCollectionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CashCollectionService(ICashCollectionRepository repository, ILogger<CashCollectionService> logger, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Llave de bloqueo compartida por las operaciones que asignan folios o cortan una caja.
    /// </summary>
    public static string GetRegisterLockKey(string cashRegisterName) => $"caja:{cashRegisterName.Trim().ToUpperInvariant()}";

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

        DateTime? fromUtc = null, toUtc = null;
        if (request.HasDateFilter())
        {
            var (inicio, fin) = request.GetNormalizedUtcRange();
            (fromUtc, toUtc) = (inicio, fin);
        }

        // Pendiente: filtro por sucursal; CashCollection no guarda la sucursal

        // Los filtros y la paginación se aplican en la base de datos
        var results = await _repository.SearchAsync(new CashCollectionSearchCriteria
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            CashRegisterName = request.HasCashRegisterFilter() ? request.CashRegisterName : null,
            IsForCashCut = request.IsCut,
            Skip = request.GetSkip(),
            Take = request.RequiresPagination() ? request.PageSize : null
        }, cancellationToken);

        _logger.LogInformation("Found {Count} cash collections matching criteria", results.Count);

        return results.Select(c => c.ToDto());
    }

    public async Task<CashCollectionDto> CreateAsync(CreateCashCollectionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // El folio se calcula y guarda bajo bloqueo para que dos recolecciones simultáneas no obtengan el mismo
            return await _unitOfWork.ExecuteLockedAsync(GetRegisterLockKey(dto.CashRegisterName), async ct =>
            {
                // Get the next folio number
                var nextFolio = await _repository.GetNextFolioAsync(dto.CashRegisterName, ct);

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

                await _repository.AddAsync(cashCollection, ct);

                _logger.LogInformation(
                    "Cash collection created. Folio: {Folio}, Register: {CashRegister}, Amount: {Amount}, IsForCashCut: {IsForCashCut}",
                    nextFolio,
                    dto.CashRegisterName,
                    cashCollection.GetTotalAmount(),
                    dto.IsForCashCut);

                return cashCollection.ToDto();
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cash collection for register {CashRegister}", dto.CashRegisterName);
            throw;
        }
    }

    public async Task<IEnumerable<CashCollectionDto>> GetPendingForCutAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        var pendingCollections = await _repository.GetPendingCollectionsByRegisterAsync(cashRegisterName, cancellationToken);
        return pendingCollections.Select(c => c.ToDto()).ToList();
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
