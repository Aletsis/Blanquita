using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;

namespace Blanquita.Infrastructure.Services;

public class SupervisorService : ISupervisorService
{
    private readonly ISupervisorRepository _repository;

    public SupervisorService(ISupervisorRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupervisorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supervisor = await _repository.GetByIdAsync(id, cancellationToken);
        return supervisor?.ToDto();
    }

    public async Task<SupervisorDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var supervisor = await _repository.GetByNameAsync(name, cancellationToken);
        return supervisor?.ToDto();
    }

    public async Task<IEnumerable<SupervisorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var supervisors = await _repository.GetAllAsync(cancellationToken);
        return supervisors.Select(s => s.ToDto());
    }

    public async Task<IEnumerable<SupervisorDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var supervisors = await _repository.GetByBranchAsync(branchId, cancellationToken);
        return supervisors.Select(s => s.ToDto());
    }

    public async Task<IEnumerable<SupervisorDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var supervisors = await _repository.GetActiveAsync(cancellationToken);
        return supervisors.Select(s => s.ToDto());
    }

    public async Task<SupervisorDto> CreateAsync(CreateSupervisorDto dto, CancellationToken cancellationToken = default)
    {
        // Check if supervisor name already exists
        if (await _repository.ExistsAsync(dto.Name, cancellationToken))
        {
            throw new DuplicateEntityException("Supervisor", dto.Name);
        }

        var supervisor = dto.ToEntity();
        await _repository.AddAsync(supervisor, cancellationToken);
        return supervisor.ToDto();
    }

    public async Task<SupervisorDto> UpdateAsync(UpdateSupervisorDto dto, CancellationToken cancellationToken = default)
    {
        var supervisor = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (supervisor == null)
        {
            throw new EntityNotFoundException("Supervisor", dto.Id);
        }

        // Check if name is being changed and if it already exists
        if (supervisor.Name != dto.Name)
        {
            if (await _repository.ExistsAsync(dto.Name, cancellationToken))
            {
                throw new DuplicateEntityException("Supervisor", dto.Name);
            }
        }

        dto.UpdateEntity(supervisor);
        await _repository.UpdateAsync(supervisor, cancellationToken);
        return supervisor.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var supervisor = await _repository.GetByIdAsync(id, cancellationToken);
        if (supervisor == null)
        {
            throw new EntityNotFoundException("Supervisor", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PagedResult<SupervisorDto>> GetPagedAsync(SearchSupervisorRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        request.Validate();

        // Obtener todos los supervisores y aplicar filtros
        var allSupervisors = await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de búsqueda
        if (request.HasSearchTerm())
        {
            var searchLower = request.SearchTerm!.ToLower();
            allSupervisors = allSupervisors.Where(s =>
                s.Name.ToLower().Contains(searchLower));
        }

        // Aplicar filtro de sucursal
        if (request.HasBranchFilter())
        {
            allSupervisors = allSupervisors.Where(s => s.BranchId == request.BranchId!.Value);
        }

        // Aplicar filtro de estado activo
        if (request.HasActiveFilter())
        {
            allSupervisors = allSupervisors.Where(s => s.IsActive == request.IsActive!.Value);
        }

        var totalCount = allSupervisors.Count();

        // Aplicar ordenamiento
        if (request.HasSorting())
        {
            allSupervisors = request.SortColumn?.ToLower() switch
            {
                "name" => request.SortAscending
                    ? allSupervisors.OrderBy(s => s.Name)
                    : allSupervisors.OrderByDescending(s => s.Name),
                "branchid" => request.SortAscending
                    ? allSupervisors.OrderBy(s => s.BranchId)
                    : allSupervisors.OrderByDescending(s => s.BranchId),
                _ => allSupervisors.OrderBy(s => s.Name)
            };
        }

        // Aplicar paginación
        var items = allSupervisors
            .Skip(request.GetSkip())
            .Take(request.PageSize);

        return PagedResult<SupervisorDto>.Create(
            items.Select(s => s.ToDto()),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
