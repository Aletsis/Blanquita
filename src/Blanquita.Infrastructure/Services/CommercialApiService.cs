using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class CommercialApiService : ICommercialApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<CommercialApiService> _logger;

    // Cache thread-safe para almacenar los tokens JWT de los usuarios y su fecha de expiración
    private static readonly ConcurrentDictionary<string, (string Token, DateTime Expires)> _tokenCache = new();

    public CommercialApiService(
        IHttpClientFactory httpClientFactory,
        IConfiguracionService configuracionService,
        ILogger<CommercialApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuracionService = configuracionService;
        _logger = logger;
    }

    private async Task<string> GetBaseUrlAsync()
    {
        var config = await _configuracionService.ObtenerConfiguracionAsync();
        if (string.IsNullOrWhiteSpace(config.CommercialApiUrl))
        {
            throw new InvalidOperationException("La dirección de la API Comercial no está configurada. Por favor, configúrela en el panel de Configuraciones.");
        }
        
        var url = config.CommercialApiUrl.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }
        
        return url.TrimEnd('/');
    }

    private async Task<string> GetApiKeyAsync()
    {
        var config = await _configuracionService.ObtenerConfiguracionAsync();
        if (string.IsNullOrWhiteSpace(config.CommercialApiKey))
        {
            throw new InvalidOperationException("El API Key de la API Comercial no está configurado. Por favor, configúrelo en el panel de Configuraciones.");
        }
        return config.CommercialApiKey.Trim();
    }

    private async Task<string> GetTokenAsync(string username)
    {
        // 1. Verificar si tenemos un token válido en la caché
        if (_tokenCache.TryGetValue(username, out var cached) && cached.Expires > DateTime.UtcNow.AddMinutes(5))
        {
            return cached.Token;
        }

        // 2. Si no, realizar petición de login
        var baseUrl = await GetBaseUrlAsync();
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);

        _logger.LogInformation("Autenticando usuario '{Username}' en la API Comercial ({BaseUrl})", username, baseUrl);

        var loginPayload = new { username = username, password = "" };
        var response = await client.PostAsJsonAsync("api/auth/login", loginPayload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Fallo al autenticar en la API Comercial para '{Username}': {Status} - {Error}", username, response.StatusCode, errorContent);
            throw new InvalidOperationException($"Error al autenticar en la API Comercial: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString();

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("La respuesta de autenticación de la API Comercial no contiene un token válido.");
        }

        // Almacenar en caché (los tokens JWT de CONTPAQi duran típicamente 8 horas)
        _tokenCache[username] = (token, DateTime.UtcNow.AddHours(7.5));

        return token;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username)
    {
        var baseUrl = await GetBaseUrlAsync();
        var token = await GetTokenAsync(username);
        var apiKey = await GetApiKeyAsync();

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        return client;
    }

    public async Task<IEnumerable<CommercialCompraDto>> GetComprasAsync(
        string? serie, 
        double? folio, 
        int? proveedorId, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta, 
        string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            
            // Construir los parámetros query
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(serie)) queryParams.Add($"serie={Uri.EscapeDataString(serie)}");
            if (folio.HasValue) queryParams.Add($"folio={folio.Value}");
            if (proveedorId.HasValue) queryParams.Add($"proveedorId={proveedorId.Value}");
            if (fechaDesde.HasValue) queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
            if (fechaHasta.HasValue) queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

            var url = "api/compras";
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            _logger.LogInformation("Consultando compras en API Comercial: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener compras de la API Comercial: {Error}", err);
                throw new Exception($"Error en la API Comercial: {response.StatusCode} - {err}");
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<CommercialCompraDto>>() ?? Enumerable.Empty<CommercialCompraDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar compras en API Comercial");
            throw;
        }
    }

    public async Task<IEnumerable<CommercialMovimientoDto>> GetMovimientosAsync(int documentoId, string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = $"api/movimientos?documentoId={documentoId}";

            _logger.LogInformation("Consultando movimientos para Documento {DocumentoId}", documentoId);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener movimientos de la API Comercial: {Error}", err);
                throw new Exception($"Error en la API: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<CommercialMovimientoDto>>() ?? Enumerable.Empty<CommercialMovimientoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar movimientos de documento {DocumentoId}", documentoId);
            throw;
        }
    }

    public async Task<IEnumerable<CommercialProveedorDto>> SearchProveedoresAsync(string? searchTerm, string username, int pageSize = 50)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = $"api/proveedores?pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            _logger.LogInformation("Buscando proveedores con término '{SearchTerm}'", searchTerm);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener proveedores: {Error}", err);
                throw new Exception($"Error en la API: {response.StatusCode}");
            }

            // El endpoint devuelve una estructura paginada { items: [], page: X, totalItems: Y, ... }
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (result.TryGetProperty("items", out var itemsProperty) && itemsProperty.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<IEnumerable<CommercialProveedorDto>>(itemsProperty.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? Enumerable.Empty<CommercialProveedorDto>();
            }

            return Enumerable.Empty<CommercialProveedorDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al buscar proveedores con término '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<CommercialProductoDto>> GetProductosAsync(string? searchTerm, string username, bool onlyActive = true)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = $"api/productos?pageSize=100000&onlyActive={onlyActive.ToString().ToLower()}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&search={Uri.EscapeDataString(searchTerm)}";
            }

            _logger.LogInformation("Buscando productos con término '{SearchTerm}'", searchTerm);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener productos: {Error}", err);
                throw new Exception($"Error en la API: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (result.TryGetProperty("items", out var itemsProperty) && itemsProperty.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<IEnumerable<CommercialProductoDto>>(itemsProperty.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? Enumerable.Empty<CommercialProductoDto>();
            }

            return Enumerable.Empty<CommercialProductoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al buscar productos con término '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<CommercialProductoDto?> GetProductoByIdAsync(int id, string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = $"api/productos/{id}";

            _logger.LogInformation("Consultando producto {Id} por ID", id);
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener producto por ID {Id}: {Error}", id, err);
                throw new Exception($"Error en la API: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<CommercialProductoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al obtener producto por ID {Id}", id);
            throw;
        }
     }

    public async Task<IEnumerable<CommercialSalidaDto>> GetSalidasAsync(
        string? serie, 
        double? folio, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta, 
        string username,
        IEnumerable<string>? allowedConcepts = null)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            
            // Construir los parámetros query
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(serie)) queryParams.Add($"serie={Uri.EscapeDataString(serie)}");
            if (folio.HasValue) queryParams.Add($"folio={folio.Value}");
            if (fechaDesde.HasValue) queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
            if (fechaHasta.HasValue) queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

            var url = "api/SalidasAlmacen";
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            _logger.LogInformation("Consultando salidas en API Comercial: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener salidas de la API Comercial: {Error}", err);
                throw new Exception($"Error en la API Comercial: {response.StatusCode} - {err}");
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CommercialSalidaDto>>() ?? Enumerable.Empty<CommercialSalidaDto>();
            
            // Filtrar en memoria por conceptos si se especificaron
            if (allowedConcepts != null && allowedConcepts.Any())
            {
                result = result.Where(s => !string.IsNullOrEmpty(s.CCODIGOCONCEPTO) && allowedConcepts.Contains(s.CCODIGOCONCEPTO)).ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar salidas en API Comercial");
            throw;
        }
    }

    public async Task<byte[]> GetSalidaPdfAsync(
        string? codigoConcepto, 
        string? serie, 
        double folio, 
        string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            
            var conceptoPath = Uri.EscapeDataString(codigoConcepto ?? "");
            var seriePath = string.IsNullOrWhiteSpace(serie) ? "-" : Uri.EscapeDataString(serie);
            
            var url = $"api/SalidasAlmacen/{conceptoPath}/{seriePath}/{folio}/pdf";
            
            _logger.LogInformation("Descargando PDF de salida de la API Comercial: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al descargar PDF de salida de la API Comercial: {Error}", err);
                throw new Exception($"Error al obtener el PDF de la API Comercial: {response.StatusCode} - {err}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al descargar PDF de salida en API Comercial");
            throw;
        }
    }

    public async Task<IEnumerable<CommercialConceptoDto>> GetConceptosAsync(
        int tipoDocumento, 
        string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = $"api/Conceptos?tipoDocumento={tipoDocumento}";

            _logger.LogInformation("Consultando conceptos en API Comercial: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener conceptos de la API Comercial: {Error}", err);
                throw new Exception($"Error en la API Comercial: {response.StatusCode} - {err}");
            }

            var rawJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Conceptos raw JSON: {RawJson}", rawJson);

            return JsonSerializer.Deserialize<IEnumerable<CommercialConceptoDto>>(rawJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<CommercialConceptoDto>();
        }


        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar conceptos en API Comercial");
            throw;
        }
    }

    public async Task<CreateCommercialSalidaResponseDto?> CreateSalidaAsync(CreateCommercialSalidaDto dto, string username)
    {
        try
        {
            var client = await CreateAuthenticatedClientAsync(username);
            var url = "api/SalidasAlmacen";
            
            _logger.LogInformation("Creando nueva salida de almacén en API Comercial: {Url}", url);
            var response = await client.PostAsJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al crear salida en la API Comercial: {Error}", err);
                throw new Exception($"Error en la API Comercial: {response.StatusCode} - {err}");
            }

            var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>();
            var resDto = new CreateCommercialSalidaResponseDto();
            
            if (jsonElement.TryGetProperty("idDocumento", out var idProp))
            {
                resDto.IdDocumento = idProp.GetInt32();
            }
            else if (jsonElement.TryGetProperty("IdDocumento", out var idPropPascal))
            {
                resDto.IdDocumento = idPropPascal.GetInt32();
            }

            if (jsonElement.TryGetProperty("codigoConcepto", out var codProp))
            {
                resDto.CodigoConcepto = codProp.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("CodigoConcepto", out var codPropPascal))
            {
                resDto.CodigoConcepto = codPropPascal.GetString() ?? string.Empty;
            }

            if (jsonElement.TryGetProperty("serie", out var serProp))
            {
                resDto.Serie = serProp.GetString() ?? string.Empty;
            }
            else if (jsonElement.TryGetProperty("Serie", out var serPropPascal))
            {
                resDto.Serie = serPropPascal.GetString() ?? string.Empty;
            }

            if (jsonElement.TryGetProperty("folio", out var folProp))
            {
                resDto.Folio = folProp.ValueKind == JsonValueKind.Number 
                    ? folProp.GetDouble().ToString() 
                    : (folProp.GetString() ?? string.Empty);
            }
            else if (jsonElement.TryGetProperty("Folio", out var folPropPascal))
            {
                resDto.Folio = folPropPascal.ValueKind == JsonValueKind.Number 
                    ? folPropPascal.GetDouble().ToString() 
                    : (folPropPascal.GetString() ?? string.Empty);
            }

            _logger.LogInformation("Salida creada exitosamente - IdDocumento: {IdDocumento}, CodigoConcepto: {CodigoConcepto}, Serie: {Serie}, Folio: {Folio}", 
                resDto.IdDocumento, resDto.CodigoConcepto, resDto.Serie, resDto.Folio);

            return resDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al crear salida en API Comercial");
            throw;
        }
    }
}




