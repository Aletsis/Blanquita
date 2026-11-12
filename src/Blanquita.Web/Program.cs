using Blanquita.Application;
using Blanquita.Infrastructure;
using Blanquita.Web.Components;
using Blanquita.Web.Extensions;
using Blanquita.Web.Services;
using DotNetEnv;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Hangfire;

// En IIS (w3wp.exe), Directory.GetCurrentDirectory() antes de CreateBuilder suele ser
// C:\Windows\System32\inetsrv, por lo que debemos buscar .env tanto desde AppContext.BaseDirectory
// como desde el directorio actual y ContentRootPath.
var initialEnvFiles = LoadDotEnvFiles(AppContext.BaseDirectory, Directory.GetCurrentDirectory());

// Usar ruta basada en AppContext.BaseDirectory para que en IIS los logs no intenten escribirse en System32\inetsrv
var logsBasePath = AppContext.BaseDirectory;

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
            path: Path.Combine(logsBasePath, "logs", "blanquita-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 104857600,
            rollOnFileSizeLimit: true,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: Path.Combine(logsBasePath, "logs", "errors", "blanquita-errors-.log"),
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

        // Asegurar que .env también se busque en ContentRootPath (establecido por IIS en CreateBuilder)
        // y que sus valores sobrescriban las cadenas vacías ("") de appsettings.json / appsettings.Production.json.
        ApplyDotEnvToConfiguration(
            builder.Configuration,
            initialEnvFiles,
            AppContext.BaseDirectory,
            builder.Environment.ContentRootPath,
            Directory.GetCurrentDirectory());

        // La cadena de conexión ya no se versiona: debe venir de .env o de variables de entorno
        if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
        {
            throw new InvalidOperationException(
                "No se configuró la cadena de conexión. Defina ConnectionStrings__DefaultConnection en el archivo .env " +
                $"o como variable de entorno (ver .env.example). Directorios verificados: BaseDirectory='{AppContext.BaseDirectory}', " +
                $"ContentRootPath='{builder.Environment.ContentRootPath}'.");
        }

        if (string.IsNullOrWhiteSpace(builder.Configuration["Encryption:Key"]))
        {
            Log.Warning("Encryption:Key no está configurada; se usa la llave por defecto incluida en el código. " +
                "Defina Encryption__Key en .env para proteger las credenciales guardadas en la base de datos.");
        }

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

        // Add Application layer (includes MediatR and handlers)
        builder.Services.AddApplication();

        // Add Infrastructure layer (includes Domain, Application, and all services)
        builder.Services.AddInfrastructure(builder.Configuration);


    var app = builder.Build();

    // Ejecutar migraciones de base de datos
    Log.Information("Verificando y migrando base de datos...");
    await app.MigrateDatabaseAsync();

    // Configurar Hangfire
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new Blanquita.Web.Extensions.HangfireDashboardAuthorizationFilter() }
    }); // Accesible en /hangfire y restringido a Admin
    await app.ConfigureRecurringJobsAsync();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    
    // Custom exception handling middleware
    app.UseMiddleware<Blanquita.Web.Middleware.ExceptionHandlingMiddleware>();
    
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

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Aplicación configurada exitosamente con Clean Architecture");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error crítico al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static List<(string FilePath, Dictionary<string, string> Values)> LoadDotEnvFiles(params string?[] startDirectories)
{
    var loadedFiles = new List<(string FilePath, Dictionary<string, string> Values)>();
    var candidateFiles = FindDotEnvFiles(startDirectories);

    foreach (var filePath in candidateFiles)
    {
        var values = ParseDotEnvFile(filePath);
        if (values.Count == 0)
        {
            continue;
        }

        foreach (var (key, value) in values)
        {
            // No sobrescribir variables ya definidas con valor en el sistema/IIS,
            // pero sí asignar si no existen o están en blanco.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        loadedFiles.Add((filePath, values));
    }

    return loadedFiles;
}

static void ApplyDotEnvToConfiguration(
    IConfigurationManager configuration,
    List<(string FilePath, Dictionary<string, string> Values)> initialLoadedFiles,
    params string?[] searchDirectories)
{
    var allLoadedFiles = new List<(string FilePath, Dictionary<string, string> Values)>(initialLoadedFiles);
    var alreadyLoadedPaths = new HashSet<string>(
        initialLoadedFiles.Select(f => f.FilePath),
        StringComparer.OrdinalIgnoreCase);

    foreach (var filePath in FindDotEnvFiles(searchDirectories))
    {
        if (alreadyLoadedPaths.Contains(filePath))
        {
            continue;
        }

        var values = ParseDotEnvFile(filePath);
        if (values.Count == 0)
        {
            continue;
        }

        foreach (var (key, value) in values)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        allLoadedFiles.Add((filePath, values));
        alreadyLoadedPaths.Add(filePath);
    }

    var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    foreach (var (filePath, values) in allLoadedFiles)
    {
        Log.Information("Archivo de configuración cargado desde {EnvFilePath}", filePath);

        foreach (var (rawKey, value) in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var configKey = rawKey.Replace("__", ConfigurationPath.KeyDelimiter);

            // Solo inyectar si en la configuración actual no hay valor (p. ej. "" en appsettings.json)
            if (string.IsNullOrWhiteSpace(configuration[configKey]) && !overrides.ContainsKey(configKey))
            {
                overrides[configKey] = value;
            }

            // Soportar también que en .env se haya definido DefaultConnection=... directamente
            if (string.Equals(configKey, "DefaultConnection", StringComparison.OrdinalIgnoreCase))
            {
                const string fullConnKey = "ConnectionStrings:DefaultConnection";
                if (string.IsNullOrWhiteSpace(configuration[fullConnKey]) && !overrides.ContainsKey(fullConnKey))
                {
                    overrides[fullConnKey] = value;
                }
            }
        }
    }

    if (overrides.Count > 0)
    {
        configuration.AddInMemoryCollection(overrides);
    }
}

static List<string> FindDotEnvFiles(IEnumerable<string?> startDirectories)
{
    var found = new List<string>();
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var startDir in startDirectories)
    {
        if (string.IsNullOrWhiteSpace(startDir))
        {
            continue;
        }

        try
        {
            var dir = new DirectoryInfo(startDir);
            while (dir is not null && dir.Exists)
            {
                var envPath = Path.Combine(dir.FullName, ".env");
                if (File.Exists(envPath) && seen.Add(envPath))
                {
                    found.Add(envPath);
                }

                dir = dir.Parent;
            }
        }
        catch
        {
            // Ignorar directorios inaccesibles al recorrer padres en IIS
        }
    }

    // Fallback en Windows por si el archivo fue guardado como .env.txt con extensiones ocultas
    if (found.Count == 0)
    {
        foreach (var startDir in startDirectories)
        {
            if (string.IsNullOrWhiteSpace(startDir))
            {
                continue;
            }

            try
            {
                var dir = new DirectoryInfo(startDir);
                while (dir is not null && dir.Exists)
                {
                    var envTxtPath = Path.Combine(dir.FullName, ".env.txt");
                    if (File.Exists(envTxtPath) && seen.Add(envTxtPath))
                    {
                        Log.Warning("No se encontró '.env', pero se detectó '{EnvTxtPath}'. Renómbrelo a '.env' sin extensión .txt.", envTxtPath);
                        found.Add(envTxtPath);
                    }

                    dir = dir.Parent;
                }
            }
            catch
            {
                // Ignorar directorios inaccesibles
            }
        }
    }

    return found;
}

static Dictionary<string, string> ParseDotEnvFile(string filePath)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    try
    {
        foreach (var kvp in Env.NoEnvVars().Load(filePath))
        {
            if (!string.IsNullOrWhiteSpace(kvp.Key))
            {
                result[kvp.Key.Trim()] = kvp.Value;
            }
        }
    }
    catch
    {
        // Si el parser estricto de DotNetEnv falla, el parser de respaldo línea por línea procesará el archivo
    }

    // Parser de respaldo línea por línea: en DotNetEnv 3.x, si un valor sin comillas contiene espacios
    // o ciertos caracteres (común en cadenas de conexión), TryParse descarta todo el archivo silenciosamente.
    try
    {
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].TrimStart();
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (!string.IsNullOrEmpty(key) &&
                (!result.TryGetValue(key, out var existing) || string.IsNullOrWhiteSpace(existing)))
            {
                result[key] = value;
            }
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "No se pudo leer el archivo de entorno en {EnvFilePath}", filePath);
    }

    return result;
}
