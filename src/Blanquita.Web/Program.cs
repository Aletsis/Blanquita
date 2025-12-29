using Blanquita.Application;
using Blanquita.Infrastructure;
using Blanquita.Web.Components;
using Blanquita.Web.Extensions;
using Blanquita.Web.Services;
using DotNetEnv;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using CurrieTechnologies.Razor.SweetAlert2;

// Cargar variables de entorno desde .env
Env.Load();

    // Configurar Serilog temprano en el proceso
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            restrictedToMinimumLevel: LogEventLevel.Information,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/blanquita-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 104857600,
            rollOnFileSizeLimit: true,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/errors/blanquita-errors-.log",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90,
            fileSizeLimitBytes: 104857600,
            rollOnFileSizeLimit: true,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
        .CreateLogger();

    try
    {
        Log.Information("Iniciando aplicación Blanquita con Clean Architecture");

        var builder = WebApplication.CreateBuilder(args);

        // Usar Serilog como el provider de logging
        builder.Host.UseSerilog();

        // Add services to the container
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddMudServices();
        builder.Services.AddControllers();
        
        builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        // UI-specific services (only services that belong to the presentation layer)
        builder.Services.AddSingleton<BrowserPrintService>();
        builder.Services.AddScoped<Blanquita.Application.Interfaces.IFileDownloadService, FileDownloadService>();
        
        builder.Services.AddSweetAlert2();

        // Add Application layer (includes MediatR and handlers)
        builder.Services.AddApplication();

        // Add Infrastructure layer (includes Domain, Application, and all services)
        builder.Services.AddInfrastructure(builder.Configuration);


    var app = builder.Build();

    // Ejecutar migraciones de base de datos
    Log.Information("Verificando y migrando base de datos...");
    await app.MigrateDatabaseAsync();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // Agregar middleware de Serilog para logging de requests HTTP
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        };
    });

    app.MapControllers();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Aplicación configurada exitosamente con Clean Architecture");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error crítico al iniciar la aplicación");
}
finally
{
    Log.CloseAndFlush();
}
