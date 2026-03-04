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
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DatabaseMigrationService(
        BlanquitaDbContext context, 
        ILogger<DatabaseMigrationService> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
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
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new ApplicationRole("Administrador"));
                _logger.LogInformation("Rol 'Administrador' creado.");
            }

            if (!await _roleManager.RoleExistsAsync("Supervisor"))
            {
                await _roleManager.CreateAsync(new ApplicationRole("Supervisor"));
                _logger.LogInformation("Rol 'Supervisor' creado.");
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
                    await _userManager.AddToRoleAsync(adminUser, "Administrador");
                    _logger.LogInformation("Usuario Administrador creado exitosamente.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al crear usuario administrador: {errors}");
                }
            }

            // Setup password and roles for migrated Supervisors (BranchId != null AND PasswordHash == null)
            var migratedSupervisors = await _userManager.Users
                .Where(u => u.BranchId != null && u.PasswordHash == null)
                .ToListAsync();

            if (migratedSupervisors.Any())
            {
                _logger.LogInformation($"Configurando contraseñas y roles para {migratedSupervisors.Count} encargadas migradas...");
                var roleName = "Supervisor";
                
                foreach (var user in migratedSupervisors)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, "Blanquita.123");
                    
                    if (result.Succeeded)
                    {
                        if (!await _userManager.IsInRoleAsync(user, roleName))
                        {
                            await _userManager.AddToRoleAsync(user, roleName);
                        }
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError($"Error al configurar contraseña para {user.UserName}: {errors}");
                    }
                }
                
                _logger.LogInformation("Encargadas migradas configuradas exitosamente.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de seeding.");
            throw;
        }
    }
}
