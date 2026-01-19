using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class EfReporteHistoricoRepository : IReporteHistoricoRepository
{
    private readonly BlanquitaDbContext _context;

    public EfReporteHistoricoRepository(BlanquitaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ReporteHistorico?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ReportesHistoricos
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<ReporteHistorico>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportesHistoricos
            .AsNoTracking()
            .Include(r => r.Detalles)
            .OrderByDescending(r => r.FechaGeneracion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReporteHistorico>> SearchAsync(
        Blanquita.Domain.ValueObjects.Sucursal? sucursal, 
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.ReportesHistoricos
            .AsNoTracking()
            .Include(r => r.Detalles)
            .AsQueryable();

        if (sucursal != null)
        {
            query = query.Where(r => r.Sucursal.Codigo == sucursal.Codigo);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(r => r.Fecha >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(r => r.Fecha <= fechaFin.Value);
        }

        return await query
            .OrderByDescending(r => r.FechaGeneracion)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        await _context.ReportesHistoricos.AddAsync(reporte, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        _context.ReportesHistoricos.Update(reporte);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var reporte = await GetByIdAsync(id, cancellationToken);
        if (reporte != null)
        {
            _context.ReportesHistoricos.Remove(reporte);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
