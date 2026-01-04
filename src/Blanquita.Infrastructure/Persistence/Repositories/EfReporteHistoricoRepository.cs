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
        string? sucursal, 
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.ReportesHistoricos
            .AsNoTracking()
            .Include(r => r.Detalles)
            .AsQueryable();

        // Note: filtering by Sucursal requires checking how it's stored. 
        // We use value converter to string (Codigo).
        // If 'sucursal' param is Name, we need to convert it?
        // Usually search param is Name or Code. The entity has Sucursal VO.
        // Assuming sucursal param is Name strings as per old implementation?
        
        if (!string.IsNullOrEmpty(sucursal))
        {
            // If the input is a name, we need to find the code.
            // Or we fetch all and filter in memory if the converter makes it hard, but that's inefficient.
            // However, with ValueConverter, EF Core translates 'r.Sucursal' to the column value (string Code).
            // So we need to compare against a Sucursal object or the converted value.
            // Since EF Core 5+, we can compare properties.
            // But 'sucursal' string might be Name.
            // Let's assume we filter by code if possible, or mapping is needed.
            
            // The previous implementation used 'Sucursal' string in DTO which was Name.
            // ReporteService (JSON) filtered by comparing strings.
            
            // To be safe and since Sucursal is a small set of values:
            var sucursalObj = Blanquita.Domain.ValueObjects.Sucursal.FromNombre(sucursal);
            if (sucursalObj != null)
            {
                 query = query.Where(r => r.Sucursal == sucursalObj);
            }
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
