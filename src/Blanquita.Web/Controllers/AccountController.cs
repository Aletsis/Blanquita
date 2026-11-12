using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Web.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAntiforgery _antiforgery;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAntiforgery antiforgery)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _antiforgery = antiforgery;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        return Redirect(BuildPageUrlWithReturnUrl("/login", returnUrl));
    }

    [HttpGet("accessdenied")]
    [AllowAnonymous]
    public IActionResult AccessDenied([FromQuery] string? returnUrl = null)
    {
        return Redirect(BuildPageUrlWithReturnUrl("/acceso-denegado", returnUrl));
    }

    [HttpPost("login")]
    [AllowAnonymous]
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
            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                var lockedUrl = "/login?error=CuentaBloqueada";
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    lockedUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
                }
                return Redirect(lockedUrl);
            }

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

    /// <summary>
    /// Cierra la sesión. Solo por POST con token antiforgery, para que un enlace o una imagen
    /// en otra página no pueda cerrar la sesión del usuario (CSRF).
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // AddControllers no registra el filtro [ValidateAntiForgeryToken]; se valida aquí
        if (!await _antiforgery.IsRequestValidAsync(HttpContext))
        {
            return Redirect("/");
        }

        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }

    private string BuildPageUrlWithReturnUrl(string basePath, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && IsSafeLocalReturnUrl(returnUrl))
        {
            return $"{basePath}?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return basePath;
    }

    private bool IsSafeLocalReturnUrl(string returnUrl)
    {
        if (Url != null)
        {
            return Url.IsLocalUrl(returnUrl);
        }

        return returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\");
    }
}
