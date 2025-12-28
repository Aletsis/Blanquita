# Sistema de Verificación y Migración de Base de Datos

## Descripción General

El sistema de verificación y migración de base de datos se ejecuta automáticamente al iniciar la aplicación Blanquita. Este sistema asegura que:

1. ✅ La base de datos existe
2. ✅ Todas las tablas necesarias están creadas
3. ✅ Todas las columnas requeridas existen en cada tabla
4. ✅ Las columnas computadas están configuradas correctamente
5. ✅ Los índices únicos están en su lugar

## Arquitectura

### Componentes Principales

#### 1. `DatabaseMigrationService`
**Ubicación**: `src/Blanquita.Infrastructure/Persistence/Migrations/DatabaseMigrationService.cs`

Este servicio es el núcleo del sistema de migraciones. Realiza las siguientes operaciones:

- **Verificación de Base de Datos**: Comprueba si la BD existe y la crea si es necesario
- **Verificación de Tablas**: Valida que todas las tablas requeridas existan
- **Verificación de Columnas**: Asegura que cada tabla tenga todas sus columnas necesarias
- **Corrección Automática**: Agrega tablas o columnas faltantes automáticamente

#### 2. `DatabaseMigrationExtensions`
**Ubicación**: `src/Blanquita.Infrastructure/Persistence/Migrations/DatabaseMigrationExtensions.cs`

Proporciona métodos de extensión para facilitar la ejecución de migraciones desde `Program.cs`:

```csharp
await app.MigrateDatabaseAsync();
```

## Tablas Gestionadas

El sistema gestiona las siguientes tablas:

### 1. Cajeras
- **Propósito**: Almacena información de las cajeras
- **Columnas**:
  - `Id` (INT, IDENTITY, PK)
  - `NumNomina` (INT, UNIQUE)
  - `Nombre` (NVARCHAR(200))
  - `Sucursal` (INT)
  - `Edo` (BIT)

### 2. Cajas
- **Propósito**: Registra las cajas registradoras
- **Columnas**:
  - `Id` (INT, IDENTITY, PK)
  - `Nombre` (NVARCHAR(200), UNIQUE)
  - `Sucursal` (INT)
  - `Ultima` (BIT)
  - `IpImpresora` (NVARCHAR(50))
  - `Port` (INT)

### 3. Encargadas
- **Propósito**: Gestiona las supervisoras
- **Columnas**:
  - `Id` (INT, IDENTITY, PK)
  - `Nombre` (NVARCHAR(200))
  - `Sucursal` (INT)
  - `Edo` (BIT)

### 4. Recolecciones
- **Propósito**: Registra las recolecciones de efectivo
- **Columnas**:
  - `Id` (INT, IDENTITY, PK)
  - `Caja` (NVARCHAR(200))
  - `Cajera` (NVARCHAR(200))
  - `Encargada` (NVARCHAR(200))
  - `FechaHora` (DATETIME2)
  - `Folio` (INT, UNIQUE)
  - `Corte` (BIT)
  - `Mil`, `Quinientos`, `Doscientos`, `Cien`, `Cincuenta`, `Veinte` (INT)
  - `CantidadTotal` (Columna Computada)

### 5. Cortes
- **Propósito**: Almacena los cortes de caja
- **Columnas**:
  - `Id` (INT, IDENTITY, PK)
  - `Caja`, `Encargada`, `Cajera`, `Sucursal` (NVARCHAR(200))
  - `FechaHora` (DATETIME2)
  - `TotalM`, `TotalQ`, `TotalD`, `TotalC`, `TotalCi`, `TotalV` (INT)
  - `TotalTira`, `TotalTarjetas` (DECIMAL(18,2))
  - `GranTotal` (Columna Computada)

## Flujo de Ejecución

```
Inicio de Aplicación
        ↓
Program.cs carga
        ↓
Configurar Serilog
        ↓
Registrar servicios (AddInfrastructure)
        ↓
Construir aplicación (app.Build())
        ↓
┌─────────────────────────────────────┐
│ MigrateDatabaseAsync()              │
├─────────────────────────────────────┤
│ 1. ¿Existe la BD?                   │
│    No → Crear BD                    │
│    Sí → Continuar                   │
│                                     │
│ 2. Verificar cada tabla:            │
│    - Cajeras                        │
│    - Cajas                          │
│    - Encargadas                     │
│    - Recolecciones                  │
│    - Cortes                         │
│                                     │
│ 3. Para cada tabla:                 │
│    ¿Existe? No → Crear tabla        │
│    Sí → Verificar columnas          │
│                                     │
│ 4. Para cada columna:               │
│    ¿Existe? No → Agregar columna    │
│    Sí → Continuar                   │
│                                     │
│ 5. Aplicar migraciones custom       │
└─────────────────────────────────────┘
        ↓
Configurar pipeline HTTP
        ↓
Iniciar aplicación
```

## Logging

El sistema registra todas las operaciones importantes:

### Nivel Information
- Inicio de verificación de BD
- BD encontrada/creada
- Verificación de tablas
- Tablas verificadas exitosamente
- Migraciones completadas

### Nivel Warning
- BD no existe (antes de crearla)
- Tabla no existe (antes de crearla)
- Columna no existe (antes de agregarla)

### Nivel Error
- Errores críticos durante la migración
- Fallos de conexión a BD
- Errores de SQL

## Ejemplo de Salida de Log

```
[16:05:39 INF] Iniciando aplicación Blanquita con Clean Architecture
[16:05:39 INF] Verificando y migrando base de datos...
[16:05:39 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: === Iniciando proceso de migración de base de datos ===
[16:05:39 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Iniciando verificación de la base de datos...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Base de datos encontrada
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tablas...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tabla Cajeras...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tabla Cajas...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tabla Encargadas...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tabla Recolecciones...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando tabla Cortes...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Todas las tablas verificadas
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificando migraciones pendientes...
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: No hay migraciones pendientes
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: Verificación de base de datos completada exitosamente
[16:05:40 INF] Blanquita.Infrastructure.Persistence.Migrations.DatabaseMigrationService: === Migración de base de datos completada exitosamente ===
```

## Manejo de Errores

El sistema está diseñado para ser resiliente:

1. **Error de Conexión**: Si no puede conectar a la BD, intenta crearla
2. **Tabla Faltante**: Crea la tabla con su estructura completa
3. **Columna Faltante**: Agrega la columna con un valor por defecto seguro
4. **Error Crítico**: Registra el error y detiene la aplicación (fail-fast)

## Agregar Nuevas Migraciones

Para agregar una nueva migración personalizada:

1. Abre `DatabaseMigrationService.cs`
2. Localiza el método `ApplyMigrationsAsync()`
3. Agrega tu lógica de migración:

```csharp
private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("Verificando migraciones pendientes...");
    
    // Ejemplo: Agregar un índice
    var indexExists = await IndexExistsAsync("Cajeras", "IX_Cajeras_Sucursal", cancellationToken);
    if (!indexExists)
    {
        _logger.LogInformation("Creando índice IX_Cajeras_Sucursal...");
        await _context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IX_Cajeras_Sucursal ON Cajeras(Sucursal)", 
            cancellationToken);
    }
    
    _logger.LogInformation("Migraciones completadas");
}
```

## Ventajas del Sistema

✅ **Automático**: No requiere intervención manual
✅ **Seguro**: Verifica antes de modificar
✅ **Idempotente**: Puede ejecutarse múltiples veces sin problemas
✅ **Transparente**: Logging detallado de todas las operaciones
✅ **Resiliente**: Manejo robusto de errores
✅ **Extensible**: Fácil agregar nuevas migraciones

## Consideraciones de Producción

- Las migraciones se ejecutan en **cada inicio** de la aplicación
- El tiempo de inicio puede aumentar ligeramente (típicamente < 1 segundo)
- En producción, considera usar un sistema de migraciones más robusto como **Entity Framework Migrations** para cambios complejos
- Este sistema es ideal para **correcciones automáticas** y **verificación de integridad**

## Troubleshooting

### La aplicación no inicia
**Causa**: Error en la migración de BD
**Solución**: Revisa los logs en `logs/errors/blanquita-errors-*.log`

### Columna duplicada
**Causa**: La columna ya existe pero el sistema intenta agregarla
**Solución**: El sistema verifica antes de agregar, esto no debería ocurrir. Si ocurre, revisa la lógica en `ColumnExistsAsync()`

### Tabla no se crea
**Causa**: Permisos insuficientes en la BD
**Solución**: Asegura que la cadena de conexión tenga permisos de DDL (CREATE TABLE, ALTER TABLE)
