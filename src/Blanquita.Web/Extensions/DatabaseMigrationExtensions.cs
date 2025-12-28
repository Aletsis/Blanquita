using Blanquita.Infrastructure.Persistence.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blanquita.Web.Extensions;

/// <summary>
/// Extensiones para facilitar la ejecución de migraciones de base de datos.
/// </summary>
public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Ejecuta las migraciones de base de datos al iniciar la aplicación.
    /// </summary>
    /// <param name="app">La aplicación web</param>
    /// <returns>La aplicación web para encadenamiento</returns>
    public static async Task<IApplicationBuilder> MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DatabaseMigrationService>>();

        try
        {
            logger.LogInformation("=== Iniciando proceso de migración de base de datos ===");
            
            var migrationService = services.GetRequiredService<DatabaseMigrationService>();
            await migrationService.EnsureDatabaseAsync();
            
            logger.LogInformation("=== Migración de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fatal durante la migración de base de datos");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Ejecuta las migraciones de base de datos de forma síncrona (para compatibilidad).
    /// </summary>
    /// <param name="app">La aplicación web</param>
    /// <returns>La aplicación web para encadenamiento</returns>
    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        return app.MigrateDatabaseAsync().GetAwaiter().GetResult();
    }
}
