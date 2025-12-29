using DbfDataReader;
using System.Text;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

/// <summary>
/// Factory para crear instancias de DbfDataReader con configuración estándar.
/// </summary>
public static class DbfReaderFactory
{
    private static readonly DbfDataReaderOptions DefaultOptions = new()
    {
        Encoding = Encoding.GetEncoding(28591), // ISO 8859-1 (Latin-1)
        SkipDeletedRecords = true
    };

    /// <summary>
    /// Crea un DbfDataReader para el archivo especificado.
    /// </summary>
    /// <param name="filePath">Ruta del archivo DBF</param>
    /// <returns>Instancia de DbfDataReader</returns>
    public static DbfDataReader.DbfDataReader CreateReader(string filePath)
    {
        var stream = File.OpenRead(filePath);
        return new DbfDataReader.DbfDataReader(stream, DefaultOptions);
    }

    /// <summary>
    /// Crea un DbfDataReader para el stream especificado.
    /// </summary>
    /// <param name="stream">Stream del archivo DBF</param>
    /// <returns>Instancia de DbfDataReader</returns>
    public static DbfDataReader.DbfDataReader CreateReader(Stream stream)
    {
        return new DbfDataReader.DbfDataReader(stream, DefaultOptions);
    }

    /// <summary>
    /// Crea un DbfDataReader con opciones personalizadas.
    /// </summary>
    /// <param name="filePath">Ruta del archivo DBF</param>
    /// <param name="options">Opciones personalizadas</param>
    /// <returns>Instancia de DbfDataReader</returns>
    public static DbfDataReader.DbfDataReader CreateReader(string filePath, DbfDataReaderOptions options)
    {
        var stream = File.OpenRead(filePath);
        return new DbfDataReader.DbfDataReader(stream, options);
    }
}
