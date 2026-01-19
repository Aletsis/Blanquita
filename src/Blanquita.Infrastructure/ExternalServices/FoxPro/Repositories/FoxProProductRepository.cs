using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a productos desde archivos FoxPro/DBF.
/// </summary>
public class FoxProProductRepository : IProductCatalogRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProProductRepository> _logger;

    public FoxProProductRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProProductRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Mgw10005Path;

        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("Ruta de archivo MGW10005 no configurada");
            return null;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo MGW10005 no encontrado: {FilePath}", filePath);
            throw new FoxProFileNotFoundException(filePath);
        }

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var productCode = reader.GetStringSafe("CCODIGOP01");
                
                if (string.Equals(productCode, code, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Producto encontrado: {Code}", code);
                    return FoxProProductMapper.MapToDto(reader);
                }
            }

            _logger.LogInformation("Producto no encontrado: {Code}", code);
            return null;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de producto cancelada: {Code}", code);
            throw;
        }
        catch (Exception ex) when (ex is not FoxProFileNotFoundException)
        {
            _logger.LogError(ex, "Error al buscar producto {Code} en FoxPro", code);
            throw new FoxProDataReadException($"Error al leer datos del producto {code}", filePath, ex);
        }
    }

    public async Task<IEnumerable<ProductSearchDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Mgw10005Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo MGW10005 no encontrado o no configurado: {FilePath}", filePath);
            return Enumerable.Empty<ProductSearchDto>();
        }

        var results = new List<ProductSearchDto>();
        // Normalize search term for easier comparison
        var term = searchTerm?.Trim();
        if (string.IsNullOrEmpty(term))
        {
            return results;
        }

        bool isNumeric = term.All(char.IsDigit);

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Read fields for comparison
                // optimization: read only what is needed for comparison first?
                // But FoxProReader likely reads the whole row or buffers it.
                
                var code = reader.GetStringSafe("CCODIGOP01");
                var name = reader.GetStringSafe("CNOMBREP01");
                var altCode = reader.GetStringSafe("CCODALTERN");
                var altName = reader.GetStringSafe("CNOMALTERN");
                var shortDesc = reader.GetStringSafe("CDESCCORTA");
                var extraText = reader.GetStringSafe("CTEXTOEX01");

                if (IsMatch(term, code, isNumeric) || 
                    IsMatch(term, name, isNumeric) || 
                    IsMatch(term, altCode, isNumeric) || 
                    IsMatch(term, altName, isNumeric) || 
                    IsMatch(term, shortDesc, isNumeric) || 
                    IsMatch(term, extraText, isNumeric))
                {
                    results.Add(FoxProProductMapper.MapToSearchDto(reader));
                }
            }

            _logger.LogInformation("Búsqueda completada. Encontrados: {Count}", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos con término '{Term}' en FoxPro", searchTerm);
             // We swallow exception here and return empty or partial results? 
             // Or throw? Usually for search UI, returning empty with log is safer than crashing.
             // But let's throw custom exception if critical.
             // For now, let's rethrow similar to GetByCode
             throw new FoxProDataReadException($"Error al buscar productos", filePath, ex);
        }
    }

    private bool IsMatch(string term, string value, bool isExact)
    {
        if (string.IsNullOrEmpty(value)) return false;
        
        if (isExact)
        {
             return string.Equals(value.Trim(), term, StringComparison.OrdinalIgnoreCase);
        }
        
        return value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
