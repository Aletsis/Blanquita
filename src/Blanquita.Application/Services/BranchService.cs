using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repository;
    private readonly ILogger<BranchService> _logger;

    public BranchService(IBranchRepository repository, ILogger<BranchService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var branches = await _repository.GetAllAsync(cancellationToken);
            return branches.Select(MapToDto);
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
            var branch = await _repository.GetByIdAsync(id, cancellationToken);
            return branch == null ? null : MapToDto(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch by id {Id}", id);
            throw;
        }
    }

    private static BranchDto MapToDto(Branch branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Code = branch.Code,
        SeriesCliente = branch.SeriesCliente,
        SeriesGlobal = branch.SeriesGlobal,
        SeriesDevolucion = branch.SeriesDevolucion,
        Direccion = branch.Direccion,
        ConceptosSalida = branch.ConceptosSalida,
        PermiteSalidasEfectivo = branch.PermiteSalidasEfectivo
    };

    public async Task<BranchDto> CreateAsync(BranchDto branchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = Branch.Create(
                branchDto.Name, 
                branchDto.Code,
                branchDto.SeriesCliente, 
                branchDto.SeriesGlobal, 
                branchDto.SeriesDevolucion,
                branchDto.Direccion,
                branchDto.ConceptosSalida,
                branchDto.PermiteSalidasEfectivo
            );

            await _repository.AddAsync(branch, cancellationToken);
            
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
            var branch = await _repository.GetByIdAsync(branchDto.Id, cancellationToken);
            if (branch == null) throw new KeyNotFoundException($"Branch with id {branchDto.Id} not found");

            branch.Update(
                branchDto.Name, 
                branchDto.Code,
                branchDto.SeriesCliente, 
                branchDto.SeriesGlobal, 
                branchDto.SeriesDevolucion,
                branchDto.Direccion,
                branchDto.ConceptosSalida,
                branchDto.PermiteSalidasEfectivo
            );

            await _repository.UpdateAsync(branch, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {Id}", branchDto.Id);
            throw;
        }
    }
}
