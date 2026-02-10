using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repository to access returns from FoxPro/DBF files.
/// </summary>
public class FoxProReturnRepository : IReturnRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProReturnRepository> _logger;

    public FoxProReturnRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProReturnRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<ReturnDto?> GetBySeriesAndFolioAsync(string series, string folio, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10008Path;

        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("POS10008 file path not configured");
            return null;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("POS10008 file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentSeries = reader.GetStringSafe("CSERIEDO01");
                var currentFolio = reader.GetStringSafe("CFOLIO");

                if (string.Equals(currentSeries?.Trim(), series?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(currentFolio?.Trim(), folio?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Return found: Series {Series}, Folio {Folio}", series, folio);
                    var returnDto = FoxProReturnMapper.MapToDto(reader);
                    
                    // Fetch details from POS10010
                    var detailsConfig = await _configService.ObtenerConfiguracionAsync(); // Or reuse 'config' variable if it has all paths
                    // The 'config' variable above already has Pos10010Path if it was updated in ConfiguracionDto
                    var detailsFilePath = config.Pos10010Path; 
                    
                    if (!string.IsNullOrEmpty(detailsFilePath) && File.Exists(detailsFilePath))
                    {
                        try 
                        {
                            using var detailReader = _readerFactory.CreateReader(detailsFilePath);
                             while (detailReader.Read())
                             {
                                 cancellationToken.ThrowIfCancellationRequested();
                                 var detailDocId = detailReader.GetStringSafe("CIDDOCUM01");
                                 
                                 if (string.Equals(detailDocId?.Trim(), returnDto.IdDocument?.Trim(), StringComparison.OrdinalIgnoreCase))
                                 {
                                     returnDto.Details.Add(FoxProReturnDetailMapper.MapToDto(detailReader));
                                 }
                             }

                            // Fetch product names from MGW10005
                            var productsFilePath = config.Mgw10005Path;
                            if (returnDto.Details.Any() && !string.IsNullOrEmpty(productsFilePath) && File.Exists(productsFilePath))
                            {
                                var productIds = returnDto.Details.Select(d => d.ProductId).Distinct().ToList();
                                var productNames = new Dictionary<string, string>();

                                try
                                {
                                    using var productReader = _readerFactory.CreateReader(productsFilePath);
                                    while (productReader.Read())
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        var prodId = productReader.GetStringSafe("CIDPRODU01");
                                        
                                        // Usually CIDPRODU01 in MGW10005 matches CIDPRODU01 in POS10010 (both are internal IDs or codes)
                                        // The user said: "lo buscaremos con el Id de producto el cual se aloja en el campo Cidprodu01"
                                        
                                        if (productIds.Contains(prodId))
                                        {
                                            var prodName = productReader.GetStringSafe("CNOMBREP01");
                                            if (!productNames.ContainsKey(prodId))
                                            {
                                                productNames.Add(prodId, prodName);
                                            }
                                        }
                                        
                                        // Optimization: If we found all distinct products, we can break
                                        if (productNames.Count == productIds.Count)
                                            break;
                                    }

                                    // Update DTOs
                                    foreach (var detail in returnDto.Details)
                                    {
                                        if (productNames.TryGetValue(detail.ProductId, out var name))
                                        {
                                            detail.ProductName = name;
                                        }
                                        else 
                                        {
                                            detail.ProductName = "Desconocido"; // Or keep empty/ID
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                     _logger.LogError(ex, "Error reading product names from MGW10005");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reading return details from POS10010");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("POS10010 file path not configured or not found: {FilePath}", detailsFilePath);
                    }

                    return returnDto;
                }
            }

            _logger.LogInformation("Return not found: Series {Series}, Folio {Folio}", series, folio);
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for return Series {Series}, Folio {Folio} in FoxPro", series, folio);
            throw;
        }
    }
}
