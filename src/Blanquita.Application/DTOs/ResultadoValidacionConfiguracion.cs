namespace Blanquita.Application.DTOs;

/// <summary>
/// Resultado de validación de configuración
/// </summary>
public class ResultadoValidacionConfiguracion
{
    /// <summary>
    /// Indica si la validación fue exitosa
    /// </summary>
    public bool EsValido { get; set; }

    /// <summary>
    /// Lista de errores de validación
    /// </summary>
    public List<string> Errores { get; set; } = new();

    /// <summary>
    /// Lista de advertencias
    /// </summary>
    public List<string> Advertencias { get; set; } = new();

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static ResultadoValidacionConfiguracion Exitoso()
    {
        return new ResultadoValidacionConfiguracion { EsValido = true };
    }

    /// <summary>
    /// Crea un resultado con errores
    /// </summary>
    public static ResultadoValidacionConfiguracion ConErrores(List<string> errores)
    {
        return new ResultadoValidacionConfiguracion
        {
            EsValido = false,
            Errores = errores
        };
    }

    /// <summary>
    /// Agrega un error
    /// </summary>
    public void AgregarError(string error)
    {
        EsValido = false;
        Errores.Add(error);
    }

    /// <summary>
    /// Agrega una advertencia
    /// </summary>
    public void AgregarAdvertencia(string advertencia)
    {
        Advertencias.Add(advertencia);
    }
}
