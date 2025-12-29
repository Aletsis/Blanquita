using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a cortes de caja desde archivos FoxPro/DBF.
/// </summary>
public class FoxProCashCutRepository : IFoxProCashCutRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProCashRegisterRepository _cashRegisterRepository;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProCashCutRepository> _logger;

    public FoxProCashCutRepository(
        IConfiguracionService configService,
        IFoxProCashRegisterRepository cashRegisterRepository,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProCashCutRepository> logger)
    {
        _configService = configService;
        _cashRegisterRepository = cashRegisterRepository;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<CashCutDto>> GetDailyCashCutsAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default)
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
