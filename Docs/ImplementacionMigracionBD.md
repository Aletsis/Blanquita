# Sistema de Verificación y Migración de Base de Datos (Versión EF Core)

## ✅ Resumen

Se ha actualizado el sistema de verificación y migración de base de datos para utilizar **EF Core Migrations**. Esto simplifica el mantenimiento, asegura la coherencia entre el código y la base de datos, y aprovecha las herramientas estándar de .NET.

## 🚀 Cómo Funciona

Al iniciar la aplicación, el `DatabaseMigrationService` ejecuta:

```csharp
await _context.Database.MigrateAsync(cancellationToken);
```

Este comando realiza automáticamente:
1.  **Creación**: Si la base de datos no existe, la crea.
2.  **Esquema**: Crea todas las tablas definidas en las migraciones (`InitialCreate`).
3.  **Actualización**: Si hay nuevas migraciones pendientes (por cambios futuros en las entidades), las aplica en orden.

## 🛠️ Mantenimiento y Evolución

### ¿Cómo agregar cambios a la base de datos?

1.  Modifica tus entidades C# en `Blanquita.Domain`.
2.  Ejecuta el comando para crear una nueva migración:
    ```bash
    dotnet ef migrations add [NombreDelCambio] --project src/Blanquita.Infrastructure --startup-project src/Blanquita.Web --output-dir Persistence/Migrations/EF
    ```
3.  ¡Listo! Al reiniciar la aplicación, los cambios se aplicarán automáticamente.

### Estructura de Proyecto

*   **Migraciones**: Se guardan en `src/Blanquita.Infrastructure/Persistence/Migrations/EF`.
*   **Servicio**: `DatabaseMigrationService` actúa como orquestador.

## 🐛 Resolución de Problemas Comunes

### "No suitable constructor" en ValueObjects
EF Core requiere constructores que pueda usar. Si agregas ValueObjects complejos, asegúrate de incluir un **constructor privado sin parámetros** para que EF Core pueda materializar los objetos.

### Conflictos de Versión
Asegúrate de que todos los paquetes `Microsoft.EntityFrameworkCore.*` estén en la misma versión mayor (actualmente **8.x** para .NET 8). No mezcles versiones 8.x con 9.x.

### "PendingModelChangesWarning"
Si ves advertencias sobre cambios pendientes en el modelo, significa que modificaste una entidad pero no creaste la migración. Ejecuta el comando `dotnet ef migrations add` descrito arriba.
