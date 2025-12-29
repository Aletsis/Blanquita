
namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public interface IFoxProDataReader : IDisposable
{
    bool Read();
    object GetValue(int ordinal);
    string GetString(int ordinal);
    decimal GetDecimal(int ordinal);
    int GetInt32(int ordinal);
    DateTime GetDateTime(int ordinal);
    int GetOrdinal(string name);
    bool IsDBNull(int ordinal);
}
