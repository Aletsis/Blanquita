using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Blanquita.Application.Interfaces;

namespace Blanquita.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR and all handlers from this assembly
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Register Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Application Services
        services.AddScoped<ICashierService, Services.CashierService>();
        services.AddScoped<IDelivererService, Services.DelivererService>();
        services.AddScoped<ICashRegisterService, Services.CashRegisterService>();
        services.AddScoped<ISupervisorService, Services.SupervisorService>();
        services.AddScoped<ICashCutService, Services.CashCutService>();
        services.AddScoped<ICashCollectionService, Services.CashCollectionService>();
        services.AddScoped<IBranchService, Services.BranchService>();
        services.AddScoped<IConfiguracionService, Services.ConfiguracionService>();
        services.AddScoped<ILabelDesignService, Services.LabelDesignService>();
        services.AddScoped<IInvoiceJobService, Services.InvoiceJobService>();
        services.AddScoped<IConciliacionService, Services.ConciliacionService>();
        services.AddScoped<IReporteHistoricoService, Services.ReporteHistoricoService>();
        
        return services;
    }
}
