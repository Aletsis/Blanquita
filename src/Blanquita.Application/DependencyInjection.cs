using Microsoft.Extensions.DependencyInjection;

namespace Blanquita.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services will be registered here
        // For now, we'll register them in the Infrastructure layer
        // since they depend on repositories
        
        return services;
    }
}
