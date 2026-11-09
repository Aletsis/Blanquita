using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbfDataReader;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public class FoxProReverseDataReader : IFoxProDataReader
{
    private const int RecordsPerBlock = 2048;

    private readonly DbfTable _table;
    private readonly DbfRecord _record;
    private readonly IList<IDbfValue> _values;
    private readonly int[] _parsedGeneration;
    private readonly int _recordLength;
    private readonly long _headerLength;
    private readonly byte[] _blockBuffer;
    private readonly Dictionary<string, int> _columnOrdinals = new(StringComparer.OrdinalIgnoreCase);

    private long _nextBlockEndRecord;
    private int _blockRecordIndex;
    private int _currentRecordOffset;
    private int _currentGeneration;
    private bool _isDisposed;

    public FoxProReverseDataReader(string filePath)
    {
        var encoding = Encoding.GetEncoding(28591); // ISO 8859-1 (Latin-1)
        
        _table = new DbfTable(filePath, encoding);
        _record = new DbfRecord(_table);
        _values = _record.Values;
        _parsedGeneration = new int[_values.Count];
        _recordLength = _table.Header.RecordLength;
        _headerLength = _table.Header.HeaderLength;
        _nextBlockEndRecord = _table.Header.RecordCount;

        var initialBlockRecords = (int)Math.Min(RecordsPerBlock, Math.Max(1L, _nextBlockEndRecord));
        _blockBuffer = _recordLength > 0 ? new byte[initialBlockRecords * _recordLength] : Array.Empty<byte>();

        for (int i = 0; i < _table.Columns.Count; i++)
        {
            _columnOrdinals[_table.Columns[i].ColumnName] = i;
        }
    }

    public bool Read()
    {
        if (_recordLength <= 0)
        {
            return false;
        }

        while (true)
        {
            if (_blockRecordIndex <= 0)
            {
                if (!ReadPreviousBlock())
                {
                    return false;
                }
            }

            _blockRecordIndex--;
            _currentRecordOffset = _blockRecordIndex * _recordLength;

            byte status = _blockBuffer[_currentRecordOffset];
            // 0x2A ('*') = registro marcado como borrado; 0x1A = marca de fin de archivo
            if (status == 0x2A || status == 0x1A)
            {
                continue;
            }

            _currentGeneration++;
            return true;
        }
    }

    private bool ReadPreviousBlock()
    {
        while (_nextBlockEndRecord > 0)
        {
            var recordsToRead = (int)Math.Min(RecordsPerBlock, _nextBlockEndRecord);
            var blockStartRecord = _nextBlockEndRecord - recordsToRead;
            _nextBlockEndRecord = blockStartRecord;

            var bytesToRead = recordsToRead * _recordLength;
            try
            {
                _table.Stream.Seek(_headerLength + (blockStartRecord * _recordLength), SeekOrigin.Begin);

                int totalRead = 0;
                while (totalRead < bytesToRead)
                {
                    int read = _table.Stream.Read(_blockBuffer, totalRead, bytesToRead - totalRead);
                    if (read <= 0)
                    {
                        break;
                    }
                    totalRead += read;
                }

                _blockRecordIndex = totalRead / _recordLength;
                if (_blockRecordIndex > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"FoxProReverseDataReader: Error reading block at record {blockStartRecord}: {ex.Message}");
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
        if (ordinal < 0 || ordinal >= _values.Count || _currentGeneration == 0)
        {
            return DBNull.Value;
        }

        var dbfValue = _values[ordinal];
        if (_parsedGeneration[ordinal] != _currentGeneration)
        {
            try
            {
                var slice = new ReadOnlySpan<byte>(_blockBuffer, _currentRecordOffset + dbfValue.Start, dbfValue.Length);
                dbfValue.Read(slice);
                _parsedGeneration[ordinal] = _currentGeneration;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"FoxProReverseDataReader: Error parsing column {ordinal}: {ex.Message}");
                return DBNull.Value;
            }
        }

        return dbfValue.GetValue() ?? DBNull.Value;
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
