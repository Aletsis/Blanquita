
namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

/// <summary>
/// Métodos de extensión para IFoxProDataReader.
/// </summary>
public static class FoxProReaderExtensions
{
    public static int GetInt32Safe(this IFoxProDataReader reader, string columnName)
    {
        try 
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return 0;
            var value = reader.GetValue(ordinal);
            return Convert.ToInt32(value);
        }
        catch 
        {
            return 0;
        }
    }

    public static string GetStringSafe(this IFoxProDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return string.Empty;
            var value = reader.GetValue(ordinal);
            return value?.ToString()?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static decimal GetDecimalSafe(this IFoxProDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return 0m;
            var value = reader.GetValue(ordinal);
            return Convert.ToDecimal(value);
        }
        catch
        {
            return 0m;
        }
    }

    public static DateTime GetDateTimeSafe(this IFoxProDataReader reader, string columnName)
    {
        try 
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return DateTime.MinValue;
            return reader.GetDateTime(ordinal);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public static double GetDoubleSafe(this IFoxProDataReader reader, string columnName)
    {
        try 
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return 0d;
            var value = reader.GetValue(ordinal);
            return Convert.ToDouble(value);
        }
        catch
        {
            return 0d;
        }
    }

    public static bool HasColumn(this IFoxProDataReader reader, string columnName)
    {
        return reader.GetOrdinal(columnName) >= 0;
    }
}
