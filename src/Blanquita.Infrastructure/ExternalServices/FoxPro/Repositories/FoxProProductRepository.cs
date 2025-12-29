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
public class FoxProProductRepository : IFoxProProductRepository
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
}
