using Blanquita.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Blanquita.Web.Tests.Controllers;

public class AccountControllerTests
{
    [Fact]
    public void LoginGet_WithLocalReturnUrl_RedirectsToLoginWithReturnUrl()
    {
        var controller = new AccountController(null!, null!, null!);

        var result = controller.Login("/consulta-conciliaciones");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/login?returnUrl=%2Fconsulta-conciliaciones", redirect.Url);
    }

    [Fact]
    public void LoginGet_WithoutReturnUrl_RedirectsToLogin()
    {
        var controller = new AccountController(null!, null!, null!);

        var result = controller.Login();

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/login", redirect.Url);
    }

    [Fact]
    public void LoginGet_WithExternalReturnUrl_IgnoresReturnUrl()
    {
        var controller = new AccountController(null!, null!, null!);

        var result = controller.Login("https://evil.example.com/phish");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/login", redirect.Url);
    }

    [Fact]
    public void AccessDeniedGet_WithLocalReturnUrl_RedirectsToAccesoDenegadoWithReturnUrl()
    {
        var controller = new AccountController(null!, null!, null!);

        var result = controller.AccessDenied("/consulta-conciliaciones");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/acceso-denegado?returnUrl=%2Fconsulta-conciliaciones", redirect.Url);
    }
}
