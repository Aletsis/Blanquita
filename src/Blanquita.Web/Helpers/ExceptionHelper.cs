using System;
using System.IO;

namespace Blanquita.Web.Helpers;

/// <summary>
/// Proporciona métodos para traducir y localizar mensajes de excepciones técnicas a mensajes amigables para el usuario.
/// </summary>
public static class ExceptionHelper
{
    /// <summary>
    /// Convierte una excepción técnica en un mensaje en español comprensible para el usuario,
    /// enfocándose especialmente en problemas comunes con archivos FoxPro/DBF e I/O.
    /// </summary>
    public static string GetLocalizedExceptionMessage(Exception ex)
    {
        if (ex == null) return "Ocurrió un error desconocido.";

        var current = ex;
        while (current != null)
        {
            if (current is FileNotFoundException fileNotFoundEx)
            {
                var fileName = !string.IsNullOrEmpty(fileNotFoundEx.FileName) 
                    ? Path.GetFileName(fileNotFoundEx.FileName) 
                    : "de base de datos";
                return $"El archivo {fileName} no fue encontrado. Verifique que las rutas DBF estén correctamente configuradas en el panel de Administración.";
            }
            
            if (current is DirectoryNotFoundException)
            {
                return "Una de las carpetas configuradas para los archivos DBF no existe o no es accesible.";
            }

            if (current is UnauthorizedAccessException)
            {
                return "Acceso denegado: El sistema no cuenta con permisos suficientes de lectura o escritura sobre los archivos DBF.";
            }

            if (current is IOException ioEx)
            {
                var message = ioEx.Message.ToLowerInvariant();
                if (message.Contains("used by another process") || 
                    message.Contains("otro proceso") || 
                    message.Contains("sharing violation") || 
                    message.Contains("proceso no tiene acceso"))
                {
                    return "El archivo DBF de CONTPAQi está temporalmente bloqueado por otro proceso (por ejemplo, el ERP de CONTPAQi o una consulta pesada). Por favor, intente de nuevo en unos segundos.";
                }
                
                return $"Error de lectura/escritura en disco: {ioEx.Message}";
            }

            if (current is TimeoutException)
            {
                return "Se agotó el tiempo de espera de la operación. Verifique la conexión de red.";
            }

            // Continuar buscando en excepciones internas (ej. excepciones arrojadas por MediatR o Reflexión)
            current = current.InnerException;
        }

        // Si no es una excepción de I/O de bajo nivel, retornar el mensaje original
        return ex.Message;
    }
}
