using DbfDataReader;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

/// <summary>
/// Métodos de extensión para DbfDataReader que facilitan la lectura de datos.
/// </summary>
public static class DbfReaderExtensions
{
    /// <summary>
    /// Obtiene un valor Int32 de forma segura desde un campo Int64.
    /// </summary>
    public static int GetInt32Safe(this DbfDataReader.DbfDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return Convert.ToInt32(reader.GetInt64(ordinal));
    }

    /// <summary>
    /// Obtiene un string trimmed de forma segura, retornando string vacío si es null.
    /// </summary>
    public static string GetStringSafe(this DbfDataReader.DbfDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetString(ordinal)?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Obtiene un decimal de forma segura.
    /// </summary>
    public static decimal GetDecimalSafe(this DbfDataReader.DbfDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        var value = reader.GetValue(ordinal);
        return Convert.ToDecimal(value);
    }

    /// <summary>
    /// Obtiene un DateTime de forma segura.
    /// </summary>
    public static DateTime GetDateTimeSafe(this DbfDataReader.DbfDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetDateTime(ordinal);
    }

    /// <summary>
    /// Verifica si una columna existe en el reader.
    /// </summary>
    public static bool HasColumn(this DbfDataReader.DbfDataReader reader, string columnName)
    {
        try
        {
            reader.GetOrdinal(columnName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
