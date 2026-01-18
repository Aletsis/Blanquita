using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Persistence.Migrations;

public class DatabaseMigrationService
{
    private readonly BlanquitaDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DatabaseMigrationService(
        BlanquitaDbContext context, 
        ILogger<DatabaseMigrationService> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task EnsureDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando migración de base de datos...");
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Migración de base de datos completada exitosamente.");

            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error durante la migración de la base de datos.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        try 
        {
            _logger.LogInformation("Verificando datos semilla...");

            // Seed Roles
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                _logger.LogInformation("Rol 'Admin' creado.");
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
                _logger.LogInformation("Rol 'User' creado.");
            }

            // Seed Admin User
            var adminEmail = "admin@blanquita.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser 
                { 
                    UserName = "admin", 
                    Email = adminEmail,
                    FullName = "Administrador del Sistema",
                    EmailConfirmed = true 
                };

                var result = await _userManager.CreateAsync(adminUser, "Blanquita.123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Usuario Administrador creado exitosamente.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al crear usuario administrador: {errors}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de seeding.");
            throw;
        }
    }
}
