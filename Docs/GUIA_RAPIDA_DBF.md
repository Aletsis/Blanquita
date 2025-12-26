# Guía Rápida: Lectura DBF con Cancelación y Streaming

## 🚀 Inicio Rápido

### 1. Búsqueda Simple con Cancelación
```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Timeout de 30 segundos

var result = await _searchService.SearchInDbfFileAsync(
    filepath: "C:\\datos\\archivo.dbf",
    fieldName: "CLIENTE",
    searchValue: "12345",
    cancellationToken: cts.Token
);

Console.WriteLine($"Encontrados: {result.MatchingRows.Count} registros");
Console.WriteLine($"Cancelado: {result.IsCancelled}");
```

### 2. Búsqueda con Límite de Memoria
```csharp
var result = await _searchService.SearchInDbfFileAsync(
    filepath: "C:\\datos\\archivo.dbf",
    fieldName: "CLIENTE",
    searchValue: "12345",
    maxMemoryMB: 50 // Máximo 50MB en memoria
);

if (result.IsPartialResult)
{
    Console.WriteLine("⚠️ Límite de memoria alcanzado");
}
```

### 3. Streaming (Bajo Uso de Memoria)
```csharp
await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
    filepath: "C:\\datos\\archivo.dbf",
    fieldName: "CLIENTE",
    searchValue: "12345",
    cancellationToken: cancellationToken))
{
    // Procesar fila por fila
    var cliente = row["CLIENTE"].ToString();
    var monto = Convert.ToDecimal(row["MONTO"]);
    
    Console.WriteLine($"Cliente: {cliente}, Monto: {monto}");
}
```

### 4. Búsqueda Múltiple
```csharp
var result = await _searchService.SearchDocsInDbfFile(
    filepath: "C:\\datos\\archivo.dbf",
    fieldNames: new[] { "FECHA", "CLIENTE" },
    searchValues: new[] { "2025-12-26", "12345" },
    progressCallback: progress => Console.WriteLine($"{progress}%"),
    maxMemoryMB: 100,
    cancellationToken: cancellationToken
);
```

## 📊 Propiedades del Resultado

```csharp
SearchResult result = await _searchService.SearchInDbfFileAsync(...);

// Datos
result.MatchingRows           // List<DataRow> - Filas encontradas
result.TotalRowsScanned       // int - Total de registros procesados

// Estado
result.IsCancelled            // bool - ¿Fue cancelado?
result.IsPartialResult        // bool - ¿Resultado incompleto?

// Métricas
result.SearchDuration         // TimeSpan - Duración de la búsqueda
result.EstimatedMemoryBytes   // long - Memoria estimada usada
```

## 🎯 Casos de Uso Comunes

### Cancelar desde UI (Blazor)
```csharp
private CancellationTokenSource? _cts;

private async Task BuscarAsync()
{
    _cts = new CancellationTokenSource();
    
    try
    {
        var result = await _searchService.SearchInDbfFileAsync(
            filepath: _dbfPath,
            fieldName: "CLIENTE",
            searchValue: _searchValue,
            cancellationToken: _cts.Token
        );
        
        _resultados = result.MatchingRows;
    }
    catch (OperationCanceledException)
    {
        _mensaje = "Búsqueda cancelada";
    }
}

private void CancelarBusqueda()
{
    _cts?.Cancel();
}
```

### API con Timeout
```csharp
[HttpGet("search")]
public async Task<IActionResult> Search(
    [FromQuery] string fieldName,
    [FromQuery] string searchValue,
    CancellationToken cancellationToken)
{
    // Timeout de 30 segundos
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken,
        timeoutCts.Token
    );

    var result = await _searchService.SearchInDbfFileAsync(
        filepath: _configuration["DbfPath"],
        fieldName: fieldName,
        searchValue: searchValue,
        maxMemoryMB: 50,
        cancellationToken: linkedCts.Token
    );

    return Ok(new
    {
        data = result.MatchingRows,
        metadata = new
        {
            totalScanned = result.TotalRowsScanned,
            duration = result.SearchDuration.TotalSeconds,
            isPartial = result.IsPartialResult
        }
    });
}
```

### Exportar a CSV
```csharp
public async Task ExportarACsvAsync(
    string dbfPath,
    string csvPath,
    CancellationToken cancellationToken)
{
    using var writer = new StreamWriter(csvPath);
    bool headerWritten = false;

    await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
        filepath: dbfPath,
        fieldName: "STATUS",
        searchValue: "ACTIVO",
        cancellationToken: cancellationToken))
    {
        if (!headerWritten)
        {
            // Escribir encabezados
            var headers = string.Join(",", 
                row.Table.Columns.Cast<DataColumn>()
                   .Select(c => c.ColumnName));
            await writer.WriteLineAsync(headers);
            headerWritten = true;
        }

        // Escribir datos
        var values = string.Join(",", 
            row.ItemArray.Select(v => $"\"{v}\""));
        await writer.WriteLineAsync(values);
    }
}
```

### Procesar por Lotes
```csharp
public async Task ProcesarPorLotesAsync(CancellationToken cancellationToken)
{
    var lote = new List<DataRow>();
    const int tamañoLote = 100;

    await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
        filepath: _dbfPath,
        fieldName: "PROCESADO",
        searchValue: "N",
        cancellationToken: cancellationToken))
    {
        lote.Add(row);
        
        if (lote.Count >= tamañoLote)
        {
            await ProcesarLoteAsync(lote);
            lote.Clear();
        }
    }
    
    // Procesar registros restantes
    if (lote.Count > 0)
    {
        await ProcesarLoteAsync(lote);
    }
}
```

## ⚙️ Parámetros Opcionales

| Parámetro | Tipo | Valor por Defecto | Descripción |
|-----------|------|-------------------|-------------|
| `chunkSize` | `int` | `1000` | Tamaño del bloque de procesamiento |
| `exactMatch` | `bool` | `true` | Búsqueda exacta vs parcial |
| `maxMemoryMB` | `int` | `500` | Límite de memoria en MB |
| `cancellationToken` | `CancellationToken` | `default` | Token de cancelación |
| `progressCallback` | `Action<int>` | `null` | Callback de progreso (0-100) |

## 💡 Mejores Prácticas

### ✅ DO
- Usar `CancellationToken` en operaciones de larga duración
- Establecer límites de memoria apropiados para tu entorno
- Usar streaming para archivos muy grandes (>100MB)
- Manejar `OperationCanceledException`
- Verificar `IsPartialResult` en los resultados

### ❌ DON'T
- No ignorar `CancellationToken` en operaciones largas
- No establecer límites de memoria muy bajos (<10MB)
- No cargar archivos gigantes (>1GB) sin streaming
- No olvidar disponer `CancellationTokenSource`

## 🔍 Troubleshooting

### Problema: "Límite de memoria alcanzado"
```csharp
// Solución 1: Aumentar límite
maxMemoryMB: 1000

// Solución 2: Usar streaming
await foreach (var row in _searchService.SearchInDbfFileStreamAsync(...))
{
    // Procesar sin acumular en memoria
}
```

### Problema: "Búsqueda muy lenta"
```csharp
// Solución 1: Reducir chunkSize para mejor responsividad
chunkSize: 500

// Solución 2: Agregar timeout
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
```

### Problema: "OutOfMemoryException"
```csharp
// Solución: SIEMPRE usar streaming para archivos grandes
await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
    filepath: largeFile,
    fieldName: "CAMPO",
    searchValue: "VALOR",
    cancellationToken: cancellationToken))
{
    // Procesar inmediatamente, no acumular
    await ProcessAndSaveAsync(row);
}
```

## 📈 Comparación de Métodos

| Método | Uso de Memoria | Velocidad | Cancelable | Mejor Para |
|--------|----------------|-----------|------------|------------|
| `SearchInDbfFileAsync` | Alto | Rápido | ✅ | Archivos pequeños/medianos |
| `SearchInDbfFileStreamAsync` | Bajo | Medio | ✅ | Archivos grandes |
| `SearchDocsInDbfFile` | Alto | Rápido | ✅ | Búsqueda múltiple |
| `SearchDocsInDbfFileStreamAsync` | Bajo | Medio | ✅ | Búsqueda múltiple en archivos grandes |

## 📚 Recursos Adicionales

- **CAMBIOS_DBF_STREAMING.md** - Documentación completa de cambios
- **EJEMPLOS_DBF_STREAMING.cs** - Ejemplos de código completos
- **RESUMEN_CAMBIOS_DBF.md** - Resumen detallado de mejoras

## 🆘 Soporte

Para preguntas o problemas:
1. Revisar los ejemplos en `EJEMPLOS_DBF_STREAMING.cs`
2. Consultar la documentación completa
3. Verificar logs de la aplicación
