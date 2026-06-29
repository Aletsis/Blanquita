using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbfDataReader;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public class FoxProReverseDataReader : IFoxProDataReader
{
    private readonly DbfTable _table;
    private readonly DbfRecord _record;
    private long _currentIndex;
    private readonly Dictionary<string, int> _columnOrdinals = new(StringComparer.OrdinalIgnoreCase);
    private bool _isDisposed;

    public FoxProReverseDataReader(string filePath)
    {
        var encoding = Encoding.GetEncoding(28591); // ISO 8859-1 (Latin-1)
        
        _table = new DbfTable(filePath, encoding);
        _record = new DbfRecord(_table);
        _currentIndex = _table.Header.RecordCount;

        for (int i = 0; i < _table.Columns.Count; i++)
        {
            _columnOrdinals[_table.Columns[i].ColumnName] = i;
        }
    }

    public bool Read()
    {
        while (_currentIndex > 0)
        {
            _currentIndex--;
            try
            {
                _table.Stream.Seek(_table.Header.HeaderLength + (_currentIndex * _table.Header.RecordLength), SeekOrigin.Begin);
                _table.Read(_record);

                if (!_record.IsDeleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"FoxProReverseDataReader: Error reading record at index {_currentIndex}: {ex.Message}");
            }
        }
        return false;
    }

    public int GetOrdinal(string name)
    {
        if (_columnOrdinals.TryGetValue(name, out var ordinal))
        {
            return ordinal;
        }
        return -1;
    }

    public object GetValue(int ordinal)
    {
        if (ordinal < 0 || ordinal >= _table.Columns.Count) return DBNull.Value;
        return _record.GetValue(ordinal);
    }

    public string GetString(int ordinal)
    {
        var val = GetValue(ordinal);
        return val == DBNull.Value || val == null ? string.Empty : val.ToString() ?? string.Empty;
    }

    public decimal GetDecimal(int ordinal)
    {
        var val = GetValue(ordinal);
        if (val == DBNull.Value || val == null) return 0m;
        try { return Convert.ToDecimal(val); } catch { return 0m; }
    }

    public int GetInt32(int ordinal)
    {
        var val = GetValue(ordinal);
        if (val == DBNull.Value || val == null) return 0;
        try { return Convert.ToInt32(val); } catch { return 0; }
    }

    public DateTime GetDateTime(int ordinal)
    {
        var val = GetValue(ordinal);
        if (val == DBNull.Value || val == null) return DateTime.MinValue;
        try { return Convert.ToDateTime(val); } catch { return DateTime.MinValue; }
    }

    public bool IsDBNull(int ordinal)
    {
        if (ordinal < 0 || ordinal >= _table.Columns.Count) return true;
        var val = GetValue(ordinal);
        return val == DBNull.Value || val == null;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _table?.Dispose();
            _isDisposed = true;
        }
    }
}
