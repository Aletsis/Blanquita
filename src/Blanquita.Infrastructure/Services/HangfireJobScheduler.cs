using Blanquita.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Programación de trabajos recurrentes con Hangfire.
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    public const string InvoiceJobId = "envio-facturas-automatico";
    public const string BackupJobId = "respaldo-base-datos-diario";

    private static readonly TimeSpan DefaultInvoiceJobTime = new(18, 0, 0);

    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobScheduler> _logger;

    public HangfireJobScheduler(IRecurringJobManager recurringJobManager, ILogger<HangfireJobScheduler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public void ConfigureRecurringJobs(TimeSpan? invoiceJobExecutionTime)
    {
        var executionTime = invoiceJobExecutionTime ?? DefaultInvoiceJobTime;

        // Convertir TimeSpan a Cron (ej. 18:30 -> "30 18 * * *")
        var cronExpression = $"{executionTime.Minutes} {executionTime.Hours} * * *";

        _recurringJobManager.AddOrUpdate<IInvoiceJobService>(
            InvoiceJobId,
            job => job.ProcessAndSendInvoicesAsync(),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local // Usar hora local del servidor
            });

        _logger.LogInformation("Trabajo recurrente de facturas configurado para ejecutarse a las {Time} (Cron: {Cron})", executionTime, cronExpression);

        // Trabajo recurrente de respaldo de base de datos diario a las 2:00 AM
        _recurringJobManager.AddOrUpdate<IDatabaseBackupService>(
            BackupJobId,
            job => job.CreateBackupAsync(null),
            "0 2 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

        _logger.LogInformation("Trabajo recurrente de respaldo de base de datos configurado para ejecutarse diariamente a las 2:00 AM");
    }
}
