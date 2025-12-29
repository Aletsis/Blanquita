namespace Blanquita.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando no se encuentra un archivo FoxPro/DBF esperado.
/// </summary>
public class FoxProFileNotFoundException : Exception
{
    public string FilePath { get; }

    public FoxProFileNotFoundException(string filePath) 
        : base($"No se encontró el archivo FoxPro: {filePath}")
    {
        FilePath = filePath;
    }

    public FoxProFileNotFoundException(string filePath, string message) : base(message)
    {
        FilePath = filePath;
    }

    public FoxProFileNotFoundException(string filePath, Exception innerException) 
        : base($"No se encontró el archivo FoxPro: {filePath}", innerException)
    {
        FilePath = filePath;
    }
}
