namespace Blanquita.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un error de conexión con archivos FoxPro/DBF.
/// </summary>
public class FoxProConnectionException : Exception
{
    public string? FilePath { get; }

    public FoxProConnectionException(string message) : base(message)
    {
    }

    public FoxProConnectionException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }

    public FoxProConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public FoxProConnectionException(string message, string filePath, Exception innerException) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
