using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a documentos desde archivos FoxPro/DBF.
/// Implementa caché en memoria para reducir lecturas repetidas del archivo.
/// </summary>
public class FoxProDocumentRepository : IFoxProDocumentRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProDocumentRepository> _logger;
    private readonly IMemoryCache _cache;

    // Configuración de caché
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "FoxProDocuments_";

    public FoxProDocumentRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProDocumentRepository> logger,
        IMemoryCache cache)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<DocumentDto>> GetByDateAndBranchAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default)
    {
        // Crear clave de caché única por fecha
        var cacheKey = $"{CacheKeyPrefix}{date:yyyyMMdd}_{branchId}";

        // Intentar obtener del caché
        if (_cache.TryGetValue(cacheKey, out IEnumerable<DocumentDto>? cachedDocuments))
        {
            _logger.LogDebug(
                "Cache HIT: Documentos para fecha {Date} obtenidos del caché ({Count} documentos)",
                date.Date,
                cachedDocuments?.Count() ?? 0);
            
            return cachedDocuments ?? Enumerable.Empty<DocumentDto>();
        }

        _logger.LogDebug("Cache MISS: Leyendo documentos desde archivo DBF para fecha {Date}", date.Date);

        // Si no está en caché, leer del archivo
        var documents = await ReadDocumentsFromFileAsync(date, branchId, cancellationToken);

        // Guardar en caché con expiración
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Size = documents.Count() // Ayuda a controlar el tamaño del caché
        };

        _cache.Set(cacheKey, documents, cacheOptions);

        _logger.LogInformation(
            "Documentos cacheados para fecha {Date} ({Count} documentos, expira en {Minutes} minutos)",
            date.Date,
            documents.Count(),
            CacheDuration.TotalMinutes);

        return documents;
    }

    private async Task<IEnumerable<DocumentDto>> ReadDocumentsFromFileAsync(
        DateTime date,
        int branchId,
        CancellationToken cancellationToken)
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
