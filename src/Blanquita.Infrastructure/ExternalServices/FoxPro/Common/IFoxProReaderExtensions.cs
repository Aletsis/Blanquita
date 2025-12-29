
namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

/// <summary>
/// Métodos de extensión para IFoxProDataReader.
/// </summary>
public static class FoxProReaderExtensions
{
    public static int GetInt32Safe(this IFoxProDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        var value = reader.GetValue(ordinal);
        return Convert.ToInt32(value);
    }

    public static string GetStringSafe(this IFoxProDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        var value = reader.GetValue(ordinal);
        return value?.ToString()?.Trim() ?? string.Empty;
    }

    public static decimal GetDecimalSafe(this IFoxProDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        var value = reader.GetValue(ordinal);
        return Convert.ToDecimal(value);
    }

    public static DateTime GetDateTimeSafe(this IFoxProDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetDateTime(ordinal);
    }
}
