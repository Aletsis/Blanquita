using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.FoxPro.DiagnoseFoxProFile;

/// <summary>
/// Handler para diagnosticar un archivo FoxPro/DBF.
/// </summary>
public class DiagnoseFoxProFileQueryHandler 
    : IRequestHandler<DiagnoseFoxProFileQuery, DiagnosticoResultado>
{
    private readonly IFoxProDiagnosticService _diagnosticService;
    private readonly ILogger<DiagnoseFoxProFileQueryHandler> _logger;

    public DiagnoseFoxProFileQueryHandler(
        IFoxProDiagnosticService diagnosticService,
        ILogger<DiagnoseFoxProFileQueryHandler> logger)
    {
        _diagnosticService = diagnosticService;
        _logger = logger;
    }

    public async Task<DiagnosticoResultado> Handle(
        DiagnoseFoxProFileQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Diagnosticando archivo: {Path}", request.Path);

        var result = await _diagnosticService.DiagnoseFileAsync(
            request.Path, 
            request.ExpectedColumns, 
            cancellationToken);

        if (result.Exitoso)
        {
            _logger.LogInformation("Diagnóstico exitoso para: {Path}", request.Path);
        }
        else
        {
            _logger.LogWarning("Diagnóstico falló para: {Path}. Errores: {Errors}", 
                request.Path, 
                string.Join(", ", result.Errores));
        }

        return result;
    }
}
