using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace Blanquita.Web.Extensions;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // En desarrollo se podría permitir acceso local sin autenticación,
        // pero para máxima seguridad en producción exigimos rol Admin.
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
