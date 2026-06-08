using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class DelivererRepository : IDelivererRepository
{
    private readonly BlanquitaDbContext _context;

    public DelivererRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<Deliverer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Deliverers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Deliverer?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Deliverers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<IEnumerable<Deliverer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Deliverers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deliverer>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var id = BranchId.Create(branchId);
        return await _context.Deliverers
            .AsNoTracking()
            .Where(c => c.BranchId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deliverer>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Deliverers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Deliverer deliverer, CancellationToken cancellationToken = default)
    {
        await _context.Deliverers.AddAsync(deliverer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Deliverer deliverer, CancellationToken cancellationToken = default)
    {
        _context.Deliverers.Update(deliverer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deliverer = await GetByIdAsync(id, cancellationToken);
        if (deliverer != null)
        {
            _context.Deliverers.Remove(deliverer);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Deliverers
            .AnyAsync(c => c.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<(IEnumerable<Deliverer> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm, 
        int page, 
        int pageSize, 
        string? sortColumn, 
        bool sortAscending, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Deliverers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) || c.EmployeeNumber.ToString().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrEmpty(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "name" => sortAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "employeenumber" => sortAscending ? query.OrderBy(c => c.EmployeeNumber) : query.OrderByDescending(c => c.EmployeeNumber),
                "branchid" => sortAscending ? query.OrderBy(c => c.BranchId) : query.OrderByDescending(c => c.BranchId),
                "isactive" => sortAscending ? query.OrderBy(c => c.IsActive) : query.OrderByDescending(c => c.IsActive),
                _ => sortAscending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id)
            };
        }
        else
        {
             query = query.OrderBy(c => c.Id);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
