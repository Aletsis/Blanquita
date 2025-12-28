using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;

namespace Blanquita.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly List<BranchDto> _branches = new()
    {
        new BranchDto { Id = 6, Name = "Himno Nacional" },
        new BranchDto { Id = 7, Name = "Pozos" },
        new BranchDto { Id = 8, Name = "Soledad" },
        new BranchDto { Id = 9, Name = "Saucito" },
        new BranchDto { Id = 10, Name = "Chapultepec" }
    };

    public Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<BranchDto>>(_branches);
    }

    public Task<BranchDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var branch = _branches.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(branch);
    }
}
