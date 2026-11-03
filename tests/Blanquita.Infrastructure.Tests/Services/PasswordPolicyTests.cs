using Blanquita.Infrastructure;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class PasswordPolicyTests
{
    // Usa la configuración real de Identity registrada por AddInfrastructure
    private static UserManager<ApplicationUser> CreateUserManager(out ServiceProvider provider)
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

        provider = services.BuildServiceProvider();
        return provider.CreateScope().ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }

    private static async Task<List<IdentityError>> ValidateAsync(string password)
    {
        var manager = CreateUserManager(out var provider);
        using (provider)
        {
            var errors = new List<IdentityError>();
            foreach (var validator in manager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(manager, new ApplicationUser { UserName = "prueba" }, password);
                errors.AddRange(result.Errors);
            }
            return errors;
        }
    }

    [Theory]
    [InlineData("Blanquita1")]
    [InlineData("Blanquita.123")] // contraseña inicial del administrador
    [InlineData("Abcdefg1")]
    public async Task ValidPasswords_ArePassed(string password)
    {
        Assert.Empty(await ValidateAsync(password));
    }

    [Theory]
    [InlineData("Abcde1", "PasswordTooShort")]
    [InlineData("blanquita1", "PasswordRequiresUpper")]
    [InlineData("BLANQUITA1", "PasswordRequiresLower")]
    [InlineData("Blanquitas", "PasswordRequiresDigit")]
    public async Task InvalidPasswords_AreRejectedWithSpanishMessage(string password, string expectedCode)
    {
        var errors = await ValidateAsync(password);

        var error = Assert.Single(errors, e => e.Code == expectedCode);
        Assert.StartsWith("La contraseña debe tener", error.Description);
    }
}
