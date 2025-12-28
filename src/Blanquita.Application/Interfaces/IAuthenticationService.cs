using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio de autenticación para el sistema
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Autentica un usuario con sus credenciales
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="password">Contraseña</param>
    /// <returns>True si la autenticación es exitosa, false en caso contrario</returns>
    Task<bool> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Valida si un usuario tiene permisos de administrador
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>True si el usuario es administrador</returns>
    Task<bool> IsAdministratorAsync(string username);

    /// <summary>
    /// Cierra la sesión del usuario actual
    /// </summary>
    Task LogoutAsync();
}
