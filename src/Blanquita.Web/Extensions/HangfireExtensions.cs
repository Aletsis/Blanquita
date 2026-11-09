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
        var jobScheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HangfireExtensions");

        try
        {
            var config = await configService.ObtenerConfiguracionAsync();
            jobScheduler.ConfigureRecurringJobs(config.InvoiceJobExecutionTime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al configurar los trabajos recurrentes de Hangfire");
        }
    }
}
