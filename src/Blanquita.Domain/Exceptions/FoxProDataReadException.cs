namespace Blanquita.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un error al leer datos de archivos FoxPro/DBF.
/// </summary>
public class FoxProDataReadException : Exception
{
    public string? FilePath { get; }
    public string? ColumnName { get; }

    public FoxProDataReadException(string message) : base(message)
    {
    }

    public FoxProDataReadException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }

    public FoxProDataReadException(string message, string filePath, string columnName) : base(message)
    {
        FilePath = filePath;
        ColumnName = columnName;
    }

    public FoxProDataReadException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public FoxProDataReadException(string message, string filePath, Exception innerException) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
