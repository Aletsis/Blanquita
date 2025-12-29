using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a documentos desde archivos FoxPro/DBF.
/// </summary>
public class FoxProDocumentRepository : IFoxProDocumentRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProDocumentRepository> _logger;

    public FoxProDocumentRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProDocumentRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentDto>> GetByDateAndBranchAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Mgw10008Path;
        var documents = new List<DocumentDto>();

        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("Ruta de archivo MGW10008 no configurada");
            return documents;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo MGW10008 no encontrado: {FilePath}", filePath);
            throw new FoxProFileNotFoundException(filePath);
        }

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var docDate = reader.GetDateTimeSafe("CFECHA");

                    if (docDate.Date == date.Date)
                    {
                        documents.Add(FoxProDocumentMapper.MapToDto(reader));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer registro de documento, continuando...");
                    continue;
                }
            }

            _logger.LogInformation(
                "Se encontraron {Count} documentos para fecha {Date}", 
                documents.Count, 
                date.Date);

            return documents;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de documentos cancelada");
            throw;
        }
        catch (Exception ex) when (ex is not FoxProFileNotFoundException)
        {
            _logger.LogError(ex, "Error al obtener documentos de FoxPro");
            throw new FoxProDataReadException("Error al leer documentos", filePath, ex);
        }
    }
}
