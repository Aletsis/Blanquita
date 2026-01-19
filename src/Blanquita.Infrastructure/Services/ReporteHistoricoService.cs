using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class ReporteHistoricoService : IReporteHistoricoService
{
    private readonly IReporteHistoricoRepository _repository;
    private readonly ILogger<ReporteHistoricoService> _logger;

    public ReporteHistoricoService(
        IReporteHistoricoRepository repository,
        ILogger<ReporteHistoricoService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task GuardarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Guardando reporte histórico para sucursal {Sucursal}", reporte.Sucursal.Nombre);
        await _repository.AddAsync(reporte, cancellationToken);
        _logger.LogInformation("Reporte guardado exitosamente con ID {Id}", reporte.Id);
    }

    public async Task<List<ReporteHistorico>> ObtenerReportesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<ReporteHistorico?> ObtenerReportePorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task EliminarReporteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando reporte histórico con ID {Id}", id);
        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<List<ReporteHistorico>> BuscarReportesAsync(BuscarReportesRequest request, CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = request.GetNormalizedDateRange();

        return await _repository.SearchAsync(request.Sucursal, inicio, fin, cancellationToken);
    }

    public async Task ActualizarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando reporte histórico con ID {Id}", reporte.Id);
        await _repository.UpdateAsync(reporte, cancellationToken);
    }
}
