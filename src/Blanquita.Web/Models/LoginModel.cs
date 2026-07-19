using System.ComponentModel.DataAnnotations;

namespace Blanquita.Web.Models;

public class LoginModel
{
    [Required(ErrorMessage = "El usuario o número de nómina es requerido")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}
