using Blanquita.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blanquita.Infrastructure.Tests.Services;

public class CookieAuthenticationConfigurationTests
{
    [Fact]
    public void AddInfrastructure_ConfiguresApplicationCookiePathsAndReturnUrlParameter()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=no_se_conecta"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var cookieOptions = optionsMonitor.Get(IdentityConstants.ApplicationScheme);

        Assert.Equal("/login", cookieOptions.LoginPath.Value);
        Assert.Equal("/Account/logout", cookieOptions.LogoutPath.Value);
        Assert.Equal("/acceso-denegado", cookieOptions.AccessDeniedPath.Value);
        Assert.Equal("returnUrl", cookieOptions.ReturnUrlParameter);
        Assert.Equal(TimeSpan.FromMinutes(5), cookieOptions.ExpireTimeSpan);
        Assert.True(cookieOptions.SlidingExpiration);
    }
}
