using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.ExternalServices.Export;
using Blanquita.Infrastructure.ExternalServices.FoxPro;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.Printing;
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
using Hangfire;
using Hangfire.PostgreSql;

namespace Blanquita.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<BlanquitaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(BlanquitaDbContext).Assembly.FullName)));

        // Memory Cache para optimizar lecturas de FoxPro
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000; // Límite de entradas en caché
            options.CompactionPercentage = 0.25; // Compactar 25% cuando se alcanza el límite
        });

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
        .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Tiempo de inactividad
            options.SlidingExpiration = true;
            options.Cookie.MaxAge = options.ExpireTimeSpan;
        });

        // Database Migration Service
        services.AddScoped<DatabaseMigrationService>();

        // Repositories
        services.AddScoped<ICashierRepository, CashierRepository>();
        services.AddScoped<IDelivererRepository, DelivererRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<ISupervisorRepository, SupervisorRepository>();
        services.AddScoped<ICashCutRepository, CashCutRepository>();
        services.AddScoped<ICashCollectionRepository, CashCollectionRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
        services.AddScoped<ILabelDesignRepository, LabelDesignRepository>();
        services.AddScoped<ISentInvoiceLogRepository, SentInvoiceLogRepository>();
        services.AddScoped<IReporteHistoricoRepository, EfReporteHistoricoRepository>();
        services.AddScoped<IConciliacionCorteRepository, ConciliacionCorteRepository>();

        // Technical Services (Stay in Infrastructure)
        services.AddHttpClient();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPrinterService, PrinterService>();
        services.AddScoped<IFileSystemService, FileSystemService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAppConfigurationManager, AppConfigurationManager>();
        services.AddScoped<ICommercialApiService, CommercialApiService>();

        // External Services
        services.AddScoped<IPrintingService, PrintingService>();
        services.AddScoped<IExportService, ExportService>();

        // FoxPro Repositories (New Architecture)
        services.AddScoped<Application.Interfaces.Repositories.IProductCatalogRepository, 
            ExternalServices.FoxPro.Repositories.FoxProProductRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProDocumentRepository, 
            ExternalServices.FoxPro.Repositories.FoxProDocumentRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProCashCutRepository, 
            ExternalServices.FoxPro.Repositories.FoxProCashCutRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProCashRegisterRepository, 
            ExternalServices.FoxPro.Repositories.FoxProCashRegisterRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProDiagnosticService, 
            ExternalServices.FoxPro.Services.FoxProDiagnosticService>();
        services.AddScoped<Application.Interfaces.Repositories.IClientCatalogRepository,
            ExternalServices.FoxPro.Repositories.FoxProClientRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IReturnRepository,
            ExternalServices.FoxPro.Repositories.FoxProReturnRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProShiftRepository,
            ExternalServices.FoxPro.Repositories.FoxProShiftRepository>();
        services.AddScoped<Application.Interfaces.Repositories.IFoxProPedidoRepository,
            ExternalServices.FoxPro.Repositories.FoxProPedidoRepository>();

        // Report Technical Services
        services.AddScoped<IReportGeneratorService, ReportGeneratorService>();
        services.AddScoped<IDbfStringParser, DbfStringParser>();
        services.AddScoped<IFoxProReaderFactory, FoxProReaderFactory>();

        // Configure FoxPro settings
        services.Configure<FoxProConfiguration>(configuration.GetSection("FoxPro"));

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => 
                options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer();

        return services;
    }
}
