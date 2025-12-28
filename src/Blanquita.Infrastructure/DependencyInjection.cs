using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Adapters;
using Blanquita.Infrastructure.ExternalServices.Export;
using Blanquita.Infrastructure.ExternalServices.FoxPro;
using Blanquita.Infrastructure.ExternalServices.Printing;
using Blanquita.Infrastructure.Persistence;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Persistence.Migrations;
using Blanquita.Infrastructure.Persistence.Repositories;
using Blanquita.Infrastructure.Services;
using Blanquita.Infrastructure.Services.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace Blanquita.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<BlanquitaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(BlanquitaDbContext).Assembly.FullName)));

        // Identity
        services.AddIdentityCore<ApplicationUser>(options => 
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<BlanquitaDbContext>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        // Database Migration Service
        services.AddScoped<DatabaseMigrationService>();

        // Repositories
        services.AddScoped<ICashierRepository, CashierRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<ISupervisorRepository, SupervisorRepository>();
        services.AddScoped<ICashCutRepository, CashCutRepository>();
        services.AddScoped<ICashCollectionRepository, CashCollectionRepository>();

        // Application Services
        services.AddScoped<ICashierService, CashierService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<ISupervisorService, SupervisorService>();
        services.AddScoped<ICashCutService, CashCutService>();
        services.AddScoped<ICashCollectionService, CashCollectionService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IPrinterService, PrinterService>();

        // Configuration Services
        services.AddSingleton<IAppConfigurationManager, AppConfigurationManager>();

        // External Services
        services.AddScoped<IFoxProReportService, FoxProReportService>();
        services.AddScoped<IPrintingService, PrintingService>();
        services.AddScoped<IExportService, ExportService>();

        // Report Services
        services.AddSingleton<IReporteService, ReporteService>();
        services.AddScoped<IReportGeneratorService, ReportGeneratorService>();
        services.AddScoped<IDbfStringParser, DbfStringParser>();
        services.AddScoped<IReporteHistoricoService, ReporteHistoricoServiceAdapter>();

        // Configure FoxPro settings
        services.Configure<FoxProConfiguration>(configuration.GetSection("FoxPro"));

        return services;
    }
}
