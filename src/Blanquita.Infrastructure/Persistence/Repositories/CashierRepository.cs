using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class CashierRepository : ICashierRepository
{
    private readonly BlanquitaDbContext _context;

    public CashierRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<Cashier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Cashiers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cashier?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Cashiers
            .AsNoTracking() // Added AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<IEnumerable<Cashier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cashiers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cashier>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var id = BranchId.Create(branchId);
        return await _context.Cashiers
            .AsNoTracking()
            .Where(c => c.BranchId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cashier>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cashiers
            .AsNoTracking() // Added AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Cashier cashier, CancellationToken cancellationToken = default)
    {
        await _context.Cashiers.AddAsync(cashier, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cashier cashier, CancellationToken cancellationToken = default)
    {
        _context.Cashiers.Update(cashier);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashier = await GetByIdAsync(id, cancellationToken);
        if (cashier != null)
        {
            _context.Cashiers.Remove(cashier);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int employeeNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Cashiers
            .AnyAsync(c => c.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<(IEnumerable<Cashier> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int page, int pageSize, string? sortColumn, bool sortAscending, CancellationToken cancellationToken = default)
    {
        var query = _context.Cashiers.AsQueryable();

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
