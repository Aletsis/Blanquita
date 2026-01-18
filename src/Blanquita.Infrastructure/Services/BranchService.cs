using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly BlanquitaDbContext _context;
    private readonly ILogger<BranchService> _logger;

    public BranchService(BlanquitaDbContext context, ILogger<BranchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Branches
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code,
                    SeriesCliente = b.SeriesCliente,
                    SeriesGlobal = b.SeriesGlobal,
                    SeriesDevolucion = b.SeriesDevolucion
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all branches");
            throw;
        }
    }

    public async Task<BranchDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(new object?[] { id }, cancellationToken);
            if (branch == null) return null;

            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Code = branch.Code,
                SeriesCliente = branch.SeriesCliente,
                SeriesGlobal = branch.SeriesGlobal,
                SeriesDevolucion = branch.SeriesDevolucion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch by id {Id}", id);
            throw;
        }
    }

    public async Task<BranchDto> CreateAsync(BranchDto branchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = Branch.Create(
                branchDto.Name, 
                branchDto.Code,
                branchDto.SeriesCliente, 
                branchDto.SeriesGlobal, 
                branchDto.SeriesDevolucion
            );

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync(cancellationToken);

            branchDto.Id = branch.Id;
            return branchDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            throw;
        }
    }

    public async Task UpdateAsync(BranchDto branchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(new object?[] { branchDto.Id }, cancellationToken);
            if (branch == null) throw new KeyNotFoundException($"Branch with id {branchDto.Id} not found");

            branch.Update(
                branchDto.Name, 
                branchDto.Code,
                branchDto.SeriesCliente, 
                branchDto.SeriesGlobal, 
                branchDto.SeriesDevolucion
            );

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {Id}", branchDto.Id);
            throw;
        }
    }
}
