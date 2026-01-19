
using Blanquita.Domain.Entities;
using Blanquita.Domain.ValueObjects;

namespace Blanquita.Domain.Repositories;

/// <summary>
/// Interfaz para el repositorio de reportes históricos.
/// </summary>
public interface IReporteHistoricoRepository
{
    Task<ReporteHistorico?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ReporteHistorico>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<List<ReporteHistorico>> SearchAsync(
        Sucursal? sucursal, 
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        CancellationToken cancellationToken = default);
        
    Task AddAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
