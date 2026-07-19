using Hangfire;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Blanquita.Web.Extensions;

public static class HangfireExtensions
{
    public static async Task ConfigureRecurringJobsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IConfiguracionService>();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HangfireExtensions");

        try
        {
            var config = await configService.ObtenerConfiguracionAsync();
            var executionTime = config.InvoiceJobExecutionTime ?? new TimeSpan(18, 0, 0);

            // Convertir TimeSpan a Cron (ej. 18:30 -> "30 18 * * *")
            string cronExpression = $"{executionTime.Minutes} {executionTime.Hours} * * *";

            recurringJobManager.AddOrUpdate<IInvoiceJobService>(
                "envio-facturas-automatico",
                job => job.ProcessAndSendInvoicesAsync(),
                cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local // Usar hora local del servidor
                });

            logger.LogInformation("Trabajo recurrente de facturas configurado para ejecutarse a las {Time} (Cron: {Cron})", executionTime, cronExpression);

            // Trabajo recurrente de respaldo de base de datos diario a las 2:00 AM
            recurringJobManager.AddOrUpdate<IDatabaseBackupService>(
                "respaldo-base-datos-diario",
                job => job.CreateBackupAsync(null),
                "0 2 * * *", // 2:00 AM todos los días
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                });

            logger.LogInformation("Trabajo recurrente de respaldo de base de datos configurado para ejecutarse diariamente a las 2:00 AM");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al configurar los trabajos recurrentes de Hangfire");
        }
    }
}
