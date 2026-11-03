using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Hangfire;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Cola de trabajos en segundo plano con Hangfire.
/// </summary>
public class HangfireBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundJobQueue(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueDescuadreAlert(DescuadreAlertDto alert)
    {
        // Hangfire sustituye CancellationToken.None por el token del trabajo al ejecutarlo
        _backgroundJobClient.Enqueue<IDescuadreAlertService>(s => s.SendAsync(alert, CancellationToken.None));
    }
}
