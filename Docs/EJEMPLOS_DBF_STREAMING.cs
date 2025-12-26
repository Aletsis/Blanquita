using Blanquita.Interfaces;
using Blanquita.Models;
using System.Data;

namespace Blanquita.Examples;

/// <summary>
/// Ejemplos de uso del servicio de búsqueda DBF mejorado con:
/// - CancellationToken support
/// - Streaming con IAsyncEnumerable
/// - Límites de memoria
/// </summary>
public class DbfSearchExamples
{
    private readonly ISearchInDbfFileService _searchService;

    public DbfSearchExamples(ISearchInDbfFileService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Ejemplo 1: Búsqueda simple con cancelación
    /// </summary>
    public async Task<SearchResult> SimpleSearchWithCancellationAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _searchService.SearchInDbfFileAsync(
                filepath: dbfPath,
                fieldName: fieldName,
                searchValue: searchValue,
                chunkSize: 1000,
                exactMatch: true,
                maxMemoryMB: 100, // Límite de 100MB
                cancellationToken: cancellationToken
            );

            Console.WriteLine($"Búsqueda completada:");
            Console.WriteLine($"- Registros escaneados: {result.TotalRowsScanned}");
            Console.WriteLine($"- Coincidencias encontradas: {result.MatchingRows.Count}");
            Console.WriteLine($"- Memoria estimada: {result.EstimatedMemoryBytes / 1024 / 1024:F2} MB");
            Console.WriteLine($"- Duración: {result.SearchDuration.TotalSeconds:F2} segundos");
            Console.WriteLine($"- Cancelado: {result.IsCancelled}");
            Console.WriteLine($"- Resultado parcial: {result.IsPartialResult}");

            return result;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("La búsqueda fue cancelada por el usuario");
            throw;
        }
    }

    /// <summary>
    /// Ejemplo 2: Búsqueda con timeout automático
    /// </summary>
    public async Task<SearchResult> SearchWithTimeoutAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        return await _searchService.SearchInDbfFileAsync(
            filepath: dbfPath,
            fieldName: fieldName,
            searchValue: searchValue,
            chunkSize: 500,
            exactMatch: false, // Búsqueda parcial
            maxMemoryMB: 50,
            cancellationToken: cts.Token
        );
    }

    /// <summary>
    /// Ejemplo 3: Búsqueda múltiple con límite de memoria
    /// </summary>
    public async Task<SearchResult> MultiFieldSearchAsync(
        string dbfPath,
        Dictionary<string, string> searchCriteria,
        int maxMemoryMB = 200)
    {
        var fieldNames = searchCriteria.Keys.ToArray();
        var searchValues = searchCriteria.Values.ToArray();

        using var cts = new CancellationTokenSource();
        
        // Callback de progreso
        void ProgressCallback(int progress)
        {
            Console.WriteLine($"Progreso: {progress}%");
        }

        var result = await _searchService.SearchDocsInDbfFile(
            filepath: dbfPath,
            fieldNames: fieldNames,
            searchValues: searchValues,
            progressCallback: ProgressCallback,
            chunkSize: 1000,
            exactMatch: true,
            maxMemoryMB: maxMemoryMB,
            cancellationToken: cts.Token
        );

        return result;
    }

    /// <summary>
    /// Ejemplo 4: Procesamiento por streaming (bajo uso de memoria)
    /// </summary>
    public async Task ProcessWithStreamingAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        CancellationToken cancellationToken)
    {
        int count = 0;
        var startTime = DateTime.UtcNow;

        try
        {
            await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
                filepath: dbfPath,
                fieldName: fieldName,
                searchValue: searchValue,
                exactMatch: true,
                cancellationToken: cancellationToken))
            {
                count++;
                
                // Procesar cada fila individualmente
                ProcessRow(row);
                
                // Mostrar progreso cada 100 registros
                if (count % 100 == 0)
                {
                    Console.WriteLine($"Procesados {count} registros...");
                }
            }

            var duration = DateTime.UtcNow - startTime;
            Console.WriteLine($"Streaming completado: {count} registros en {duration.TotalSeconds:F2} segundos");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Streaming cancelado después de procesar {count} registros");
            throw;
        }
    }

    /// <summary>
    /// Ejemplo 5: Streaming con múltiples campos
    /// </summary>
    public async Task<List<DataRow>> StreamMultiFieldSearchAsync(
        string dbfPath,
        string[] fieldNames,
        string[] searchValues,
        int maxResults = 1000,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DataRow>();

        await foreach (var row in _searchService.SearchDocsInDbfFileStreamAsync(
            filepath: dbfPath,
            fieldNames: fieldNames,
            searchValues: searchValues,
            exactMatch: true,
            cancellationToken: cancellationToken))
        {
            results.Add(row);
            
            // Limitar resultados para evitar consumo excesivo de memoria
            if (results.Count >= maxResults)
            {
                Console.WriteLine($"Límite de {maxResults} resultados alcanzado");
                break;
            }
        }

        return results;
    }

    /// <summary>
    /// Ejemplo 6: Búsqueda con reintentos en caso de error
    /// </summary>
    public async Task<SearchResult?> SearchWithRetryAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                
                return await _searchService.SearchInDbfFileAsync(
                    filepath: dbfPath,
                    fieldName: fieldName,
                    searchValue: searchValue,
                    chunkSize: 1000,
                    exactMatch: true,
                    maxMemoryMB: 100,
                    cancellationToken: cts.Token
                );
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"Intento {attempt} falló: {ex.Message}");
                Console.WriteLine($"Reintentando en 2 segundos...");
                await Task.Delay(2000);
            }
        }

        Console.WriteLine($"Búsqueda falló después de {maxRetries} intentos");
        return null;
    }

    /// <summary>
    /// Ejemplo 7: Exportar resultados a CSV con streaming
    /// </summary>
    public async Task ExportToCsvStreamingAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        string outputCsvPath,
        CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(outputCsvPath);
        bool headerWritten = false;
        int rowCount = 0;

        await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
            filepath: dbfPath,
            fieldName: fieldName,
            searchValue: searchValue,
            exactMatch: true,
            cancellationToken: cancellationToken))
        {
            // Escribir encabezados
            if (!headerWritten)
            {
                var headers = string.Join(",", row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                await writer.WriteLineAsync(headers);
                headerWritten = true;
            }

            // Escribir datos
            var values = string.Join(",", row.ItemArray.Select(v => $"\"{v}\""));
            await writer.WriteLineAsync(values);
            rowCount++;
        }

        Console.WriteLine($"Exportados {rowCount} registros a {outputCsvPath}");
    }

    private void ProcessRow(DataRow row)
    {
        // Implementar lógica de procesamiento aquí
        // Por ejemplo: validar, transformar, guardar en base de datos, etc.
    }
}

/// <summary>
/// Ejemplo de uso en un servicio web o API
/// </summary>
public class DbfApiExample
{
    private readonly ISearchInDbfFileService _searchService;

    public DbfApiExample(ISearchInDbfFileService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Endpoint de API con soporte de cancelación
    /// </summary>
    public async Task<IEnumerable<DataRow>> SearchEndpointAsync(
        string dbfPath,
        string fieldName,
        string searchValue,
        CancellationToken httpContextCancellationToken)
    {
        // Combinar timeout con cancelación del contexto HTTP
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            httpContextCancellationToken,
            timeoutCts.Token
        );

        var result = await _searchService.SearchInDbfFileAsync(
            filepath: dbfPath,
            fieldName: fieldName,
            searchValue: searchValue,
            chunkSize: 500,
            exactMatch: true,
            maxMemoryMB: 50, // Límite bajo para API
            cancellationToken: linkedCts.Token
        );

        if (result.IsPartialResult)
        {
            Console.WriteLine("Advertencia: Resultado parcial debido a límites de memoria o tiempo");
        }

        return result.MatchingRows;
    }
}
