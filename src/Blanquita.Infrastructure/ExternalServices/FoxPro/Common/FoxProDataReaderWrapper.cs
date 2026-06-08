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

    public object GetValue(int ordinal) => ordinal >= 0 ? _reader.GetValue(ordinal) : DBNull.Value;

    public string GetString(int ordinal) => ordinal >= 0 ? _reader.GetString(ordinal) : string.Empty;

    public decimal GetDecimal(int ordinal) => ordinal >= 0 ? _reader.GetDecimal(ordinal) : 0m;

    public int GetInt32(int ordinal) => ordinal >= 0 ? _reader.GetInt32(ordinal) : 0;

    public DateTime GetDateTime(int ordinal) => ordinal >= 0 ? _reader.GetDateTime(ordinal) : DateTime.MinValue;

    public int GetOrdinal(string name)
    {
        try
        {
            return _reader.GetOrdinal(name);
        }
        catch
        {
            return -1;
        }
    }

    public bool IsDBNull(int ordinal)
    {
        if (ordinal < 0) return true;
        
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
