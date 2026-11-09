namespace Blanquita.Application.Interfaces;

/// <summary>
/// Programa los trabajos recurrentes del sistema (envío de facturas, respaldos).
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Registra o actualiza los trabajos recurrentes. El envío de facturas se programa a la hora indicada
    /// (hora local del servidor); si es null se usa la hora predeterminada.
    /// </summary>
    void ConfigureRecurringJobs(TimeSpan? invoiceJobExecutionTime);
}
