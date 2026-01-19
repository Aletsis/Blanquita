using DbfDataReader;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public class FoxProDataReaderWrapper : IFoxProDataReader
{
    private readonly DbfDataReader.DbfDataReader _reader;

    public FoxProDataReaderWrapper(DbfDataReader.DbfDataReader reader)
    {
        _reader = reader;
    }

    public bool Read() => _reader.Read();

    public object GetValue(int ordinal) => _reader.GetValue(ordinal);

    public string GetString(int ordinal) => _reader.GetString(ordinal);

    public decimal GetDecimal(int ordinal) => _reader.GetDecimal(ordinal);

    public int GetInt32(int ordinal) => _reader.GetInt32(ordinal);

    public DateTime GetDateTime(int ordinal) => _reader.GetDateTime(ordinal);

    public int GetOrdinal(string name) => _reader.GetOrdinal(name);

    public bool IsDBNull(int ordinal)
    {
        // DbfDataReader methods might throw or return generic values, handling requires care.
        // DbfDataReader doesn't have IsDBNull easily exposed in same way as IDataRecord usually.
        // We rely on GetValue returning DBNull.Value or null
        try
        {
            var value = _reader.GetValue(ordinal);
            return value == null || value == DBNull.Value;
        }
        catch (ArgumentOutOfRangeException)
        {
            // If the column index is out of range for this specific record (corrupt/truncated row),
            // treat it as NULL safe to skip.
            return true;
        }
        catch (IndexOutOfRangeException)
        {
            return true;
        }
        catch
        {
            // Any other error reading the value implies we should treat it as null/missing
            return true;
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
