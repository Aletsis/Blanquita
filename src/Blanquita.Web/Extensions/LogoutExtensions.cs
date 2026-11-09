using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Blanquita.Web.Extensions;

public static class LogoutExtensions
{
    /// <summary>
    /// Envía el POST de cierre de sesión con el token antiforgery del circuito actual.
    /// </summary>
    public static ValueTask SubmitLogoutAsync(this IJSRuntime js, AntiforgeryStateProvider antiforgery)
    {
        var token = antiforgery.GetAntiforgeryToken();
        return js.InvokeVoidAsync("submitLogoutForm", "/Account/logout", token?.FormFieldName, token?.Value);
    }
}
