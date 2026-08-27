using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Web.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            var errorUrl = "/login?error=DatosRequeridos";
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                errorUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
            }
            return Redirect(errorUrl);
        }

        ApplicationUser? user = await _userManager.FindByNameAsync(username);

        if (user == null && int.TryParse(username, out int employeeNumber))
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeNumber == employeeNumber);
        }

        if (user != null)
        {
            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) &&
                        string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        return Redirect(uri.PathAndQuery);
                    }
                }

                return Redirect("/");
            }
        }

        var invalidCredsUrl = "/login?error=CredencialesInvalidas";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            invalidCredsUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
        return Redirect(invalidCredsUrl);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }
}
