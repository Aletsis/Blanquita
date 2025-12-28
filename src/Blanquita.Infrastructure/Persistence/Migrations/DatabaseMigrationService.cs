using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Blanquita.Infrastructure.Persistence.Identity;

namespace Blanquita.Infrastructure.Persistence.Migrations;

/// <summary>
/// Servicio responsable de verificar y migrar la base de datos al iniciar la aplicación
/// utilizando EF Core Migrations.
/// </summary>
public class DatabaseMigrationService
{
    private readonly BlanquitaDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        BlanquitaDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Aplica cualquier migración pendiente a la base de datos.
    /// Crea la base de datos si no existe.
    /// </summary>
    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando verificación de base de datos con EF Core...");

            // Esta llamada verifica si la base de datos existe, la crea si no,
            // y aplica todas las migraciones pendientes en orden.
            await _context.Database.MigrateAsync(cancellationToken);

            await SeedDefaultUserAsync();
            
            _logger.LogInformation("Base de datos actualizada correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al aplicar migraciones de base de datos.");
            throw;
        }
    }


    private async Task SeedDefaultUserAsync()
    {
        try 
        {
            var adminUser = await _userManager.FindByNameAsync("Admin");
            if (adminUser == null)
            {
                _logger.LogInformation("Creando usuario Administrador por defecto...");
                var user = new ApplicationUser 
                { 
                    UserName = "Admin", 
                    Email = "admin@blanquita.com", 
                    EmailConfirmed = true,
                    FullName = "Administrador Sistema"
                };
                
                var result = await _userManager.CreateAsync(user, "Blanquita.123");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario Administrador creado exitosamente.");
                }
                else
                {
                    _logger.LogError("Error al crear usuario Admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error al seedear datos iniciales.");
        }
    }
}
