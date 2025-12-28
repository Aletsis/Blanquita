using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;

namespace Blanquita.Infrastructure.Services;

public class CashierService : ICashierService
{
    private readonly ICashierRepository _repository;

    public CashierService(ICashierRepository repository)
    {
        _repository = repository;
    }

    public async Task<CashierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashier = await _repository.GetByIdAsync(id, cancellationToken);
        return cashier?.ToDto();
    }

    public async Task<CashierDto?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        var cashier = await _repository.GetByEmployeeNumberAsync(employeeNumber, cancellationToken);
        return cashier?.ToDto();
    }

    public async Task<IEnumerable<CashierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cashiers = await _repository.GetAllAsync(cancellationToken);
        return cashiers.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<CashierDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var cashiers = await _repository.GetByBranchAsync(branchId, cancellationToken);
        return cashiers.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<CashierDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var cashiers = await _repository.GetActiveAsync(cancellationToken);
        return cashiers.Select(c => c.ToDto());
    }

    public async Task<CashierDto> CreateAsync(CreateCashierDto dto, CancellationToken cancellationToken = default)
    {
        // Check if employee number already exists
        if (await _repository.ExistsAsync(dto.EmployeeNumber, cancellationToken))
        {
            throw new DuplicateEntityException("Cashier", $"Employee Number {dto.EmployeeNumber}");
        }

        var cashier = dto.ToEntity();
        await _repository.AddAsync(cashier, cancellationToken);
        return cashier.ToDto();
    }

    public async Task<CashierDto> UpdateAsync(UpdateCashierDto dto, CancellationToken cancellationToken = default)
    {
        var cashier = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (cashier == null)
        {
            throw new EntityNotFoundException("Cashier", dto.Id);
        }

        // Check if employee number is being changed and if it already exists
        if (cashier.EmployeeNumber != dto.EmployeeNumber)
        {
            if (await _repository.ExistsAsync(dto.EmployeeNumber, cancellationToken))
            {
                throw new DuplicateEntityException("Cashier", $"Employee Number {dto.EmployeeNumber}");
            }
        }

        dto.UpdateEntity(cashier);
        await _repository.UpdateAsync(cashier, cancellationToken);
        return cashier.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashier = await _repository.GetByIdAsync(id, cancellationToken);
        if (cashier == null)
        {
            throw new EntityNotFoundException("Cashier", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PagedResult<CashierDto>> GetPagedAsync(SearchCashierRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        request.Validate();

        // Aplicar filtros del request
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            request.SortColumn,
            request.SortAscending,
            cancellationToken);

        // Filtrar por BranchId si se especificó
        if (request.HasBranchFilter())
        {
            items = items.Where(c => c.BranchId == request.BranchId!.Value);
            totalCount = items.Count();
        }

        // Filtrar por IsActive si se especificó
        if (request.HasActiveFilter())
        {
            items = items.Where(c => c.IsActive == request.IsActive!.Value);
            totalCount = items.Count();
        }

        // Filtrar por EmployeeNumber si se especificó
        if (request.HasEmployeeNumberFilter())
        {
            items = items.Where(c => c.EmployeeNumber == request.EmployeeNumber!.Value);
            totalCount = items.Count();
        }

        return PagedResult<CashierDto>.Create(
            items.Select(c => c.ToDto()),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
