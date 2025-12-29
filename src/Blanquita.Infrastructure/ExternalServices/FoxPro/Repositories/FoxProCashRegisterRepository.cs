using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a cajas registradoras desde archivos FoxPro/DBF.
/// </summary>
public class FoxProCashRegisterRepository : IFoxProCashRegisterRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProCashRegisterRepository> _logger;

    public FoxProCashRegisterRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProCashRegisterRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<string> GetNameByIdAsync(int cashRegisterId, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10041Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10041 no disponible: {FilePath}", filePath);
            return string.Empty;
        }

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var id = reader.GetInt32Safe("CIDCAJA");
                
                if (id == cashRegisterId)
                {
                    var name = reader.GetStringSafe("CSERIENOTA");
                    _logger.LogDebug("Caja registradora encontrada: ID={Id}, Nombre={Name}", id, name);
                    return name;
                }
            }

            _logger.LogDebug("Caja registradora no encontrada: ID={Id}", cashRegisterId);
            return string.Empty;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de caja registradora cancelada");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener nombre de caja registradora {Id}", cashRegisterId);
            return string.Empty; // Retornar vacío en lugar de lanzar excepción para no romper el flujo
        }
    }
}
