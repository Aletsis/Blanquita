# Estrategia de Logging - Blanquita

## Resumen

Este proyecto utiliza **Serilog** como framework de logging con configuración centralizada y niveles de log apropiados para cada componente.

## Configuración de Serilog

### Niveles de Log

Los niveles de log están configurados jerárquicamente:

- **Default**: `Information` - Nivel base para toda la aplicación
- **Microsoft**: `Warning` - Reduce ruido de logs del framework
- **Microsoft.Hosting.Lifetime**: `Information` - Eventos importantes del ciclo de vida
- **Microsoft.EntityFrameworkCore**: `Warning` - Solo errores y advertencias de EF Core
- **Microsoft.AspNetCore**: `Warning` - Solo errores y advertencias de ASP.NET Core
- **System**: `Warning` - Reduce logs del sistema
- **System.Net.Http.HttpClient**: `Warning` - Reduce logs de HTTP
- **Blanquita**: `Information` - Logs de la aplicación
- **Blanquita.Services.FoxProService**: `Information` - Operaciones con FoxPro
- **Blanquita.Services.ReportGeneratorService**: `Information` - Generación de reportes
- **Blanquita.Services.SearchInDbfFileService**: `Information` - Búsquedas en DBF

### Política de Retención

#### Logs Generales
- **Ubicación**: `logs/blanquita-YYYYMMDD.log`
- **Rotación**: Diaria
- **Retención**: 30 días
- **Tamaño máximo por archivo**: 100 MB
- **Roll on size limit**: Sí (crea nuevo archivo si se excede el tamaño)

#### Logs de Errores
- **Ubicación**: `logs/errors/blanquita-errors-YYYYMMDD.log`
- **Rotación**: Diaria
- **Retención**: 90 días (3 meses)
- **Nivel mínimo**: Error
- **Tamaño máximo por archivo**: 100 MB
- **Roll on size limit**: Sí

### Destinos de Log (Sinks)

1. **Console**
   - Nivel mínimo: `Information`
   - Formato: `[HH:mm:ss LEVEL] SourceContext: Message`
   - Uso: Desarrollo y debugging

2. **File (General)**
   - Todos los niveles desde `Information`
   - Incluye propiedades estructuradas
   - Formato completo con timestamp, nivel, contexto, mensaje y propiedades

3. **File (Errors)**
   - Solo `Error` y `Critical`
   - Retención extendida para análisis histórico
   - Mismo formato que logs generales

## Guía de Uso de Niveles de Log

### Trace
**Uso**: Información muy detallada para debugging profundo
```csharp
_logger.LogTrace("Scanning field index {Index}", i);
```
**Cuándo usar**: Loops internos, valores de variables en cada iteración

### Debug
**Uso**: Información de debugging útil durante desarrollo
```csharp
_logger.LogDebug("Series - Cliente: {SerieCliente}, Global: {SerieGlobal}", series.Cliente, series.Global);
_logger.LogDebug("Procesando corte - Caja ID: {IdCaja}, Serie: '{SerieCaja}'", corte.IdCaja, corte.SerieCaja);
```
**Cuándo usar**: 
- Detalles de procesamiento iterativo
- Valores de configuración
- Pasos intermedios en algoritmos

### Information
**Uso**: Eventos importantes del flujo normal de la aplicación
```csharp
_logger.LogInformation("Iniciando generación de reporte para {Sucursal} en fecha {Fecha:dd/MM/yyyy}", sucursal, fecha);
_logger.LogInformation("Total de cortes encontrados: {Count}", cortes.Count);
_logger.LogInformation("Impresora seleccionada - ID: {IdImpresora}, IP: {IpImpresora}", id, ip);
```
**Cuándo usar**:
- Inicio/fin de operaciones importantes
- Resultados de operaciones (conteos, totales)
- Selección de recursos (impresoras, archivos)
- Eventos de negocio significativos

### Warning
**Uso**: Situaciones anormales que no impiden el funcionamiento
```csharp
_logger.LogWarning("Registro {RowNumber} tiene solo {FieldCount} campos. Se esperaban {ExpectedCount}", 
    rowNumber, fieldCount, expectedCount);
```
**Cuándo usar**:
- Datos corruptos que se pueden omitir
- Configuración subóptima
- Uso de valores por defecto
- Situaciones recuperables

### Error
**Uso**: Errores que impiden una operación específica
```csharp
_logger.LogError(ex, "Error al obtener cortes");
_logger.LogError("Mismatch: {FieldCount} fields != {ValueCount} values", fieldCount, valueCount);
```
**Cuándo usar**:
- Excepciones capturadas
- Operaciones fallidas
- Validaciones fallidas críticas
- Errores de red/IO recuperables

### Critical
**Uso**: Errores críticos que pueden afectar toda la aplicación
```csharp
_logger.LogCritical(ex, "Error crítico en registro #{RowNumber}", rowNumber);
Log.Fatal(ex, "Error crítico al iniciar la aplicación");
```
**Cuándo usar**:
- Fallas al iniciar la aplicación
- Pérdida de conexión a recursos críticos
- Corrupción de datos críticos
- Situaciones irrecuperables

## Logging Estructurado

Serilog soporta logging estructurado. Usa placeholders en lugar de interpolación de strings:

### ✅ Correcto
```csharp
_logger.LogInformation("Usuario {UserId} procesó {Count} registros en {Elapsed}ms", 
    userId, count, elapsed);
```

### ❌ Incorrecto
```csharp
_logger.LogInformation($"Usuario {userId} procesó {count} registros en {elapsed}ms");
```

## Enriquecimiento de Logs

Los logs se enriquecen automáticamente con:
- **EnvironmentName**: Development/Production
- **MachineName**: Nombre del servidor
- **ProcessId**: ID del proceso
- **ThreadId**: ID del thread
- **SourceContext**: Clase que genera el log

## Inyección de ILogger

Todos los servicios deben inyectar `ILogger<T>` en el constructor:

```csharp
public class MiServicio
{
    private readonly ILogger<MiServicio> _logger;
    
    public MiServicio(ILogger<MiServicio> logger)
    {
        _logger = logger;
    }
    
    public void MiMetodo()
    {
        _logger.LogInformation("Ejecutando MiMetodo");
    }
}
```

## Monitoreo y Análisis

### Archivos de Log
- Los logs generales se encuentran en `logs/blanquita-YYYYMMDD.log`
- Los logs de errores se encuentran en `logs/errors/blanquita-errors-YYYYMMDD.log`
- Los archivos más antiguos se eliminan automáticamente según la política de retención

### Búsqueda en Logs
Los logs están en formato estructurado, facilitando búsquedas:
```bash
# Buscar todos los errores de un servicio específico
grep "FoxProService" logs/errors/blanquita-errors-*.log

# Buscar operaciones de una fecha específica
grep "2025-12-26" logs/blanquita-20251226.log

# Buscar por nivel
grep "ERR" logs/blanquita-*.log
```

## Mejores Prácticas

1. **No usar Console.WriteLine**: Siempre usar `ILogger`
2. **Usar niveles apropiados**: Information para operaciones, Debug para detalles
3. **Logging estructurado**: Usar placeholders, no interpolación
4. **Incluir contexto**: Agregar IDs, nombres, valores relevantes
5. **No loggear información sensible**: Contraseñas, tokens, datos personales
6. **Usar excepciones**: Pasar la excepción como primer parámetro en LogError/LogCritical
7. **Mensajes descriptivos**: Explicar qué pasó, no solo "Error"
8. **Consistencia**: Usar el mismo formato para operaciones similares

## Configuración por Ambiente

### Development
- Console muestra todos los logs desde Information
- Logs detallados para debugging
- Retención de 30 días

### Production
- Console solo muestra Information y superior
- Logs de errores con retención de 90 días
- Tamaño de archivo limitado para evitar problemas de disco

## Cambios Recientes

### 2025-12-26
- ✅ Reemplazados todos los `Console.WriteLine` por `ILogger`
- ✅ Configurada retención de 30 días para logs generales
- ✅ Configurada retención de 90 días para logs de errores
- ✅ Agregado límite de tamaño de archivo (100 MB)
- ✅ Creado sink separado para errores
- ✅ Optimizados niveles de log en servicios principales
- ✅ Agregados overrides para reducir ruido de frameworks

## Referencias

- [Serilog Documentation](https://serilog.net/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [Structured Logging](https://github.com/serilog/serilog/wiki/Structured-Data)
