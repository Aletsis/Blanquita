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
/// Repositorio para acceder a cortes de caja desde archivos FoxPro/DBF.
/// Implementa caché en memoria para reducir lecturas repetidas del archivo.
/// </summary>
public class FoxProCashCutRepository : IFoxProCashCutRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProCashRegisterRepository _cashRegisterRepository;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProCashCutRepository> _logger;
    private readonly IMemoryCache _cache;

    // Configuración de caché
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "FoxProCashCuts_";

    public FoxProCashCutRepository(
        IConfiguracionService configService,
        IFoxProCashRegisterRepository cashRegisterRepository,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProCashCutRepository> logger,
        IMemoryCache cache)
    {
        _configService = configService;
        _cashRegisterRepository = cashRegisterRepository;
        _readerFactory = readerFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<CashCutDto>> GetDailyCashCutsAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default)
    {
        // Crear clave de caché única por fecha
        var cacheKey = $"{CacheKeyPrefix}{date:yyyyMMdd}_{branchId}";

        // Intentar obtener del caché
        if (_cache.TryGetValue(cacheKey, out IEnumerable<CashCutDto>? cachedCuts))
        {
            _logger.LogDebug(
                "Cache HIT: Cortes de caja para fecha {Date} obtenidos del caché ({Count} cortes)",
                date.Date,
                cachedCuts?.Count() ?? 0);
            
            return cachedCuts ?? Enumerable.Empty<CashCutDto>();
        }

        _logger.LogDebug("Cache MISS: Leyendo cortes de caja desde archivo DBF para fecha {Date}", date.Date);

        // Si no está en caché, leer del archivo
        var cashCuts = await ReadCashCutsFromFileAsync(date, branchId, cancellationToken);

        // Guardar en caché con expiración
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Size = cashCuts.Count() // Ayuda a controlar el tamaño del caché
        };

        _cache.Set(cacheKey, cashCuts, cacheOptions);

        _logger.LogInformation(
            "Cortes de caja cacheados para fecha {Date} ({Count} cortes, expira en {Minutes} minutos)",
            date.Date,
            cashCuts.Count(),
            CacheDuration.TotalMinutes);

        return cashCuts;
    }

    private async Task<IEnumerable<CashCutDto>> ReadCashCutsFromFileAsync(
        DateTime date,
        int branchId,
        CancellationToken cancellationToken)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10042Path;
        var cashCuts = new List<CashCutDto>();

        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("Ruta de archivo POS10042 no configurada");
            return cashCuts;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10042 no encontrado: {FilePath}", filePath);
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
                    var cutDate = reader.GetDateTimeSafe("CFECHACOR");

                    if (cutDate.Date == date.Date)
                    {
                        var cashRegisterId = reader.GetInt32Safe("CIDCAJA");
                        var cashRegisterName = await _cashRegisterRepository.GetNameByIdAsync(
                            cashRegisterId, 
                            cancellationToken);

                        if (!string.IsNullOrEmpty(cashRegisterName))
                        {
                            cashCuts.Add(FoxProCashCutMapper.MapToDto(reader, cashRegisterName, branchId));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer registro de corte de caja, continuando...");
                    continue;
                }
            }

            _logger.LogInformation(
                "Se encontraron {Count} cortes de caja para fecha {Date}", 
                cashCuts.Count, 
                date.Date);

            return cashCuts;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de cortes de caja cancelada");
            throw;
        }
        catch (Exception ex) when (ex is not FoxProFileNotFoundException)
        {
            _logger.LogError(ex, "Error al obtener cortes de caja de FoxPro");
            throw new FoxProDataReadException("Error al leer cortes de caja", filePath, ex);
        }
    }
}
