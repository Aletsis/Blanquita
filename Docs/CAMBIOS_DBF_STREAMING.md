# Mejoras en Lectura de Archivos DBF

## Objetivo
Agregar capacidades de cancelación, streaming y límites de memoria al servicio de lectura de archivos DBF.

## Cambios Implementados

### 1. **CancellationToken Support**
- Agregado `CancellationToken` a todos los métodos de lectura DBF
- Permite cancelar operaciones de larga duración
- Verifica cancelación en puntos estratégicos del procesamiento

### 2. **Streaming con IAsyncEnumerable**
- Nuevos métodos que retornan `IAsyncEnumerable<DataRow>` para procesamiento por streaming
- Permite procesar registros uno por uno sin cargar todo en memoria
- Ideal para archivos DBF muy grandes

### 3. **Límites de Memoria**
- Nuevo parámetro `maxMemoryMB` para controlar el uso de memoria
- Monitoreo del tamaño de los chunks en memoria
- Limpieza automática cuando se alcanza el límite

### 4. **Mejoras en el Modelo SearchResult**
- Agregado `IsCancelled` para indicar si la búsqueda fue cancelada
- Agregado `IsPartialResult` para indicar si hay más datos disponibles
- Mejor tracking del progreso y estado

## Métodos Nuevos

### `SearchInDbfFileStreamAsync`
Versión streaming de `SearchInDbfFileAsync` que retorna `IAsyncEnumerable<DataRow>`.

### `SearchDocsInDbfFileStreamAsync`
Versión streaming de `SearchDocsInDbfFile` que retorna `IAsyncEnumerable<DataRow>`.

## Uso

### Ejemplo 1: Búsqueda con Cancelación
```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Cancelar después de 30 segundos

var result = await _searchService.SearchInDbfFileAsync(
    filepath: "path/to/file.dbf",
    fieldName: "CLIENTE",
    searchValue: "12345",
    chunkSize: 1000,
    exactMatch: true,
    maxMemoryMB: 100,
    cancellationToken: cts.Token
);
```

### Ejemplo 2: Procesamiento por Streaming
```csharp
await foreach (var row in _searchService.SearchInDbfFileStreamAsync(
    filepath: "path/to/file.dbf",
    fieldName: "CLIENTE",
    searchValue: "12345",
    cancellationToken: cancellationToken))
{
    // Procesar cada fila individualmente
    ProcessRow(row);
}
```

### Ejemplo 3: Búsqueda Múltiple con Límite de Memoria
```csharp
var result = await _searchService.SearchDocsInDbfFile(
    filepath: "path/to/file.dbf",
    fieldNames: new[] { "FECHA", "CLIENTE" },
    searchValues: new[] { "2025-12-26", "12345" },
    progressCallback: progress => Console.WriteLine($"Progreso: {progress}%"),
    chunkSize: 500,
    exactMatch: true,
    maxMemoryMB: 50,
    cancellationToken: cancellationToken
);
```

## Beneficios

1. **Mejor Control**: Cancelación de operaciones largas
2. **Menor Uso de Memoria**: Streaming y límites configurables
3. **Mayor Escalabilidad**: Procesar archivos muy grandes sin problemas
4. **Mejor UX**: Feedback de progreso y capacidad de cancelar
5. **Más Robusto**: Manejo de errores mejorado con limpieza de recursos

## Compatibilidad

- Los métodos existentes mantienen su firma original
- Se agregaron sobrecargas con parámetros opcionales
- Totalmente compatible con código existente
