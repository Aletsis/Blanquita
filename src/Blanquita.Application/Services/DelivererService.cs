using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;

namespace Blanquita.Application.Services;

public class DelivererService : IDelivererService
{
    private readonly IDelivererRepository _repository;

    public DelivererService(IDelivererRepository repository)
    {
        _repository = repository;
    }

    public async Task<DelivererDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var deliverer = await _repository.GetByIdAsync(id, cancellationToken);
        return deliverer?.ToDto();
    }

    public async Task<DelivererDto?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        var deliverer = await _repository.GetByEmployeeNumberAsync(employeeNumber, cancellationToken);
        return deliverer?.ToDto();
    }

    public async Task<IEnumerable<DelivererDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var deliverers = await _repository.GetAllAsync(cancellationToken);
        return deliverers.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<DelivererDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var deliverers = await _repository.GetByBranchAsync(branchId, cancellationToken);
        return deliverers.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<DelivererDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var deliverers = await _repository.GetActiveAsync(cancellationToken);
        return deliverers.Select(c => c.ToDto());
    }

    public async Task<DelivererDto> CreateAsync(CreateDelivererDto dto, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsAsync(dto.EmployeeNumber, cancellationToken))
        {
            throw new DuplicateEntityException("Deliverer", $"Employee Number {dto.EmployeeNumber}");
        }

        var deliverer = dto.ToEntity();
        await _repository.AddAsync(deliverer, cancellationToken);
        return deliverer.ToDto();
    }

    public async Task<DelivererDto> UpdateAsync(UpdateDelivererDto dto, CancellationToken cancellationToken = default)
    {
        var deliverer = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (deliverer == null)
        {
            throw new EntityNotFoundException("Deliverer", dto.Id);
        }

        if (deliverer.EmployeeNumber != dto.EmployeeNumber)
        {
            if (await _repository.ExistsAsync(dto.EmployeeNumber, cancellationToken))
            {
                throw new DuplicateEntityException("Deliverer", $"Employee Number {dto.EmployeeNumber}");
            }
        }

        dto.UpdateEntity(deliverer);
        await _repository.UpdateAsync(deliverer, cancellationToken);
        return deliverer.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deliverer = await _repository.GetByIdAsync(id, cancellationToken);
        if (deliverer == null)
        {
            throw new EntityNotFoundException("Deliverer", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PagedResult<DelivererDto>> GetPagedAsync(SearchDelivererRequest request, CancellationToken cancellationToken = default)
    {
        request.Validate();

        var (items, totalCount) = await _repository.GetPagedAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            request.SortColumn,
            request.SortAscending,
            cancellationToken);

        if (request.HasBranchFilter())
        {
            items = items.Where(c => c.BranchId.Value == request.BranchId!.Value);
            totalCount = items.Count();
        }

        if (request.HasActiveFilter())
        {
            items = items.Where(c => c.IsActive == request.IsActive!.Value);
            totalCount = items.Count();
        }

        if (request.HasEmployeeNumberFilter())
        {
            items = items.Where(c => c.EmployeeNumber == request.EmployeeNumber!.Value);
            totalCount = items.Count();
        }

        return PagedResult<DelivererDto>.Create(
            items.Select(c => c.ToDto()),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
