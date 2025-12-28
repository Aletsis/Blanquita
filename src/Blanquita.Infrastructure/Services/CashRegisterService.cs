using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Mappings;
using Blanquita.Domain.Exceptions;
using Blanquita.Domain.Repositories;

namespace Blanquita.Infrastructure.Services;

public class CashRegisterService : ICashRegisterService
{
    private readonly ICashRegisterRepository _repository;

    public CashRegisterService(ICashRegisterRepository repository)
    {
        _repository = repository;
    }

    public async Task<CashRegisterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashRegister = await _repository.GetByIdAsync(id, cancellationToken);
        return cashRegister?.ToDto();
    }

    public async Task<CashRegisterDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var cashRegister = await _repository.GetByNameAsync(name, cancellationToken);
        return cashRegister?.ToDto();
    }

    public async Task<IEnumerable<CashRegisterDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cashRegisters = await _repository.GetAllAsync(cancellationToken);
        return cashRegisters.Select(c => c.ToDto());
    }

    public async Task<IEnumerable<CashRegisterDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var cashRegisters = await _repository.GetByBranchAsync(branchId, cancellationToken);
        return cashRegisters.Select(c => c.ToDto());
    }

    public async Task<CashRegisterDto> CreateAsync(CreateCashRegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Check if cash register name already exists
        if (await _repository.ExistsAsync(dto.Name, cancellationToken))
        {
            throw new DuplicateEntityException("CashRegister", dto.Name);
        }

        // If this is marked as the last register, unset any existing last register for the branch
        if (dto.IsLastRegister)
        {
            var existingLastRegister = await _repository.GetLastRegisterByBranchAsync(dto.BranchId, cancellationToken);
            if (existingLastRegister != null)
            {
                existingLastRegister.UnsetAsLastRegister();
                await _repository.UpdateAsync(existingLastRegister, cancellationToken);
            }
        }

        var cashRegister = dto.ToEntity();
        await _repository.AddAsync(cashRegister, cancellationToken);
        return cashRegister.ToDto();
    }

    public async Task<CashRegisterDto> UpdateAsync(UpdateCashRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var cashRegister = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (cashRegister == null)
        {
            throw new EntityNotFoundException("CashRegister", dto.Id);
        }

        // Check if name is being changed and if it already exists
        if (cashRegister.Name != dto.Name)
        {
            if (await _repository.ExistsAsync(dto.Name, cancellationToken))
            {
                throw new DuplicateEntityException("CashRegister", dto.Name);
            }
        }

        // If this is being set as the last register, unset any existing last register for the branch
        if (dto.IsLastRegister && !cashRegister.IsLastRegister)
        {
            var existingLastRegister = await _repository.GetLastRegisterByBranchAsync(dto.BranchId, cancellationToken);
            if (existingLastRegister != null && existingLastRegister.Id != dto.Id)
            {
                existingLastRegister.UnsetAsLastRegister();
                await _repository.UpdateAsync(existingLastRegister, cancellationToken);
            }
        }

        dto.UpdateEntity(cashRegister);
        await _repository.UpdateAsync(cashRegister, cancellationToken);
        return cashRegister.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashRegister = await _repository.GetByIdAsync(id, cancellationToken);
        if (cashRegister == null)
        {
            throw new EntityNotFoundException("CashRegister", id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<CashRegisterDto?> GetBackupRegisterAsync(int currentRegisterId, CancellationToken cancellationToken = default)
    {
        var current = await GetByIdAsync(currentRegisterId, cancellationToken);
        if (current == null) return null;

        int backupId = current.IsLastRegister ? current.Id - 1 : current.Id + 1;
        return await GetByIdAsync(backupId, cancellationToken);
    }

    public async Task<PagedResult<CashRegisterDto>> GetPagedAsync(SearchCashRegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        request.Validate();

        // Obtener todas las cajas y aplicar filtros
        var allRegisters = await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de búsqueda
        if (request.HasSearchTerm())
        {
            var searchLower = request.SearchTerm!.ToLower();
            allRegisters = allRegisters.Where(r =>
                r.Name.ToLower().Contains(searchLower) ||
                (r.PrinterConfig != null && r.PrinterConfig.IpAddress.ToLower().Contains(searchLower)));
        }

        // Aplicar filtro de sucursal
        if (request.HasSucursalFilter())
        {
            // Aquí necesitarías mapear el Value Object Sucursal a BranchId
            // Por ahora, asumiendo que Sucursal tiene una forma de obtener el ID
            var branchName = request.Sucursal!.Nombre;
            allRegisters = allRegisters.Where(r => r.BranchId.ToString() == branchName);
        }

        // Aplicar filtro de nombre de caja
        if (request.HasCashRegisterNameFilter())
        {
            allRegisters = allRegisters.Where(r => r.Name.Equals(request.CashRegisterName, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = allRegisters.Count();

        // Aplicar ordenamiento
        if (request.HasSorting())
        {
            allRegisters = request.SortColumn?.ToLower() switch
            {
                "name" => request.SortAscending
                    ? allRegisters.OrderBy(r => r.Name)
                    : allRegisters.OrderByDescending(r => r.Name),
                "branchid" => request.SortAscending
                    ? allRegisters.OrderBy(r => r.BranchId)
                    : allRegisters.OrderByDescending(r => r.BranchId),
                "printerip" => request.SortAscending
                    ? allRegisters.OrderBy(r => r.PrinterConfig.IpAddress)
                    : allRegisters.OrderByDescending(r => r.PrinterConfig.IpAddress),
                _ => allRegisters.OrderBy(r => r.Name)
            };
        }

        // Aplicar paginación
        var items = allRegisters
            .Skip(request.GetSkip())
            .Take(request.PageSize);

        return PagedResult<CashRegisterDto>.Create(
            items.Select(r => r.ToDto()),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
