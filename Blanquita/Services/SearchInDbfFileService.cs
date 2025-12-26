using System.Data.Common;
using System.Data;
using System.Text;
using DbfDataReader;
using Blanquita.Interfaces;
using Blanquita.Models;

namespace Blanquita.Services
{
    public class SearchInDbfFileService : ISearchInDbfFileService
    {
        private readonly ILogger<SearchInDbfFileService> _logger;

        public SearchInDbfFileService(ILogger<SearchInDbfFileService> logger)
        {
            _logger = logger;
        }
        public async Task<SearchResult> SearchInDbfFileAsync(
    string filepath,
    string fieldName,
    string searchValue,
    int chunkSize = 1000,
    bool exactMatch = true)
        {
            var dataTable = new DataTable();
            var result = new SearchResult
            {
                MatchingRows = new List<DataRow>(),
                TotalRowsScanned = 0
            };

            var options = new DbfDataReaderOptions
            {
                SkipDeletedRecords = true,
                Encoding = GetBestEncodingForDbf(),
                //UseMemoFile = false  ← ¡Importante! Ignora el archivo de memos
            };
            dataTable.TableName = Path.GetFileNameWithoutExtension(filepath);
            try
            {
                // 1. Validar que el archivo y campo existan
                if (!File.Exists(filepath))
                    throw new FileNotFoundException("El archivo DBF no existe", filepath);

                // 2. Abrir el archivo y verificar el campo
                using (var dbfReader = new DbfDataReader.DbfDataReader(filepath, options))
                {
                    var fieldExists = false;
                    int fieldIndex = -1;
                    Type fieldType = null;

                    // Verificar si el campo existe y obtener su tipo
                    for (int i = 0; i < dbfReader.FieldCount; i++)
                    {
                        _logger.LogTrace("Scanning field index {Index}", i);
                        if (dbfReader.GetName(i).Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                        {
                            fieldExists = true;
                            fieldIndex = i;
                            fieldType = dbfReader.GetFieldType(i);
                            break;
                        }
                    }

                    if (!fieldExists)
                        throw new ArgumentException($"El campo '{fieldName}' no existe en el archivo DBF");

                    // 3. Contar registros para progreso (opcional, puede comentarse para más velocidad)
                    int totalRecords = 0;
                    while (dbfReader.Read()) totalRecords++;
                    dbfReader.Dispose();


                    // 4. Reabrir para realizar la búsqueda
                    using (var reader = new DbfDataReader.DbfDataReader(filepath, options))
                    {
                        // 5. Primero solo leer la estructura
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            _logger.LogDebug("Field: {FieldName}, Type: {FieldType}", reader.GetName(i), reader.GetFieldType(i));
                            var fieldTypes = MapDbfTypeToNetType(reader.GetFieldType(i));
                            dataTable.Columns.Add(reader.GetName(i), fieldType);
                        }
                        var currentChunk = new List<DataRow>(chunkSize);
                        int processedRecords = 0;

                        while (reader.Read())
                        {
                            processedRecords++;
                            result.TotalRowsScanned++;

                            // Actualizar progreso
                            if (totalRecords > 0 && processedRecords % 100 == 0)
                            {
                                int progress = (int)((double)processedRecords / totalRecords * 100);
                                await Task.Yield(); // Mantener la UI responsive
                            }

                            // Obtener el valor del campo
                            var fieldValue = reader.GetValue(fieldIndex)?.ToString();

                            // Realizar la comparación
                            bool isMatch;
                            if (exactMatch)
                            {
                                isMatch = string.Equals(fieldValue, searchValue, StringComparison.OrdinalIgnoreCase);
                            }
                            else
                            {
                                isMatch = fieldValue?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false;
                            }

                            // Si encontramos coincidencia
                            if (isMatch)
                            {
                                // Crear DataRow con todos los campos
                                _logger.LogTrace("Creating DataRow with all fields");
                                var row = dataTable.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i) ?? DBNull.Value;
                                }
                                currentChunk.Add(row);

                                // Procesar chunk si está lleno
                                if (currentChunk.Count >= chunkSize)
                                {
                                    await ProcessChunkAsync(dataTable, currentChunk);
                                    result.MatchingRows.AddRange(currentChunk);
                                    currentChunk.Clear();
                                }
                            }
                        }

                        // Agregar las filas restantes
                        if (currentChunk.Count > 0)
                        {
                            result.MatchingRows.AddRange(currentChunk);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la búsqueda");
                throw;
            }
        }
        public async Task<SearchResult> SearchDocsInDbfFile(
    string filepath,
    string[] fieldNames,  // Cambiado a array de nombres de campos
    string[] searchValues, // Cambiado a array de valores de búsqueda
    Action<int> progressCallback = null,
    int chunkSize = 1000,
    bool exactMatch = true)
        {
            var dataTable = new DataTable();
            var result = new SearchResult
            {
                MatchingRows = new List<DataRow>(),
                TotalRowsScanned = 0
            };

            // Validar que el número de campos y valores coincida
            if (fieldNames.Length != searchValues.Length)
                throw new ArgumentException("El número de campos y valores de búsqueda debe coincidir");
            _logger.LogError("Mismatch: {FieldCount} fields != {ValueCount} values", fieldNames.Length, searchValues.Length);

            var options = new DbfDataReaderOptions
            {
                SkipDeletedRecords = true,
                Encoding = GetBestEncodingForDbf()
            };
            dataTable.TableName = Path.GetFileNameWithoutExtension(filepath);

            try
            {
                if (!File.Exists(filepath))
                    throw new FileNotFoundException("El archivo DBF no existe", filepath);

                using (var dbfReader = new DbfDataReader.DbfDataReader(filepath, options))
                {
                    // Verificar que todos los campos existan y obtener sus índices y tipos
                    _logger.LogDebug("Verificando que los campos existan, obtener sus indices y tipos");
                    var fieldIndices = new int[fieldNames.Length];
                    var fieldTypes = new Type[fieldNames.Length];

                    for (int j = 0; j < fieldNames.Length; j++)
                    {
                        bool fieldExists = false;
                        for (int i = 0; i < dbfReader.FieldCount; i++)
                        {
                            if (dbfReader.GetName(i).Equals(fieldNames[j], StringComparison.OrdinalIgnoreCase))
                            {
                                fieldExists = true;
                                fieldIndices[j] = i;
                                fieldTypes[j] = dbfReader.GetFieldType(i);
                                break;
                            }
                        }

                        if (!fieldExists)
                            throw new ArgumentException($"El campo '{fieldNames[j]}' no existe en el archivo DBF");
                    }

                    // Contar registros para progreso
                    int totalRecords = 0;
                    //while (dbfReader.Read()) totalRecords++;
                    dbfReader.Dispose();

                    // Reabrir para realizar la búsqueda
                    using (var reader = new DbfDataReader.DbfDataReader(filepath, options))
                    {
                        // Crear estructura de la tabla
                        dataTable.Clear();
                        dataTable.Columns.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type fieldType = MapDbfTypeToNetType(reader.GetFieldType(i));
                            dataTable.Columns.Add(reader.GetName(i), fieldType);
                        }
                        var currentChunk = new List<DataRow>(chunkSize);
                        int processedRecords = 0;
                        var cont = 0;
                        while (true)
                        {
                            try
                            {
                                if (!reader.Read())
                                    break;
                                cont++;
                                result.TotalRowsScanned++;
                                try
                                {

                                    if (reader.FieldCount < fieldIndices.Max() + 1)
                                    {
                                        _logger.LogWarning("Registro {TotalRowsScanned} tiene solo {FieldCount} campos. Se esperaban al menos {ExpectedCount}.", result.TotalRowsScanned, reader.FieldCount, fieldIndices.Max() + 1);
                                        continue;
                                    }
                                    bool allFieldsMatch = true;

                                    // 1. Validar estructura del registro actual
                                    if (reader.FieldCount != dataTable.Columns.Count)
                                    {
                                        _logger.LogWarning("Fila {Count} tiene {FieldCount} campos (se esperaban {ExpectedCount}). Omitiendo.", cont, reader.FieldCount, dataTable.Columns.Count);
                                        continue;
                                    }

                                    // 2. Búsqueda en campos
                                    for (int j = 0; j < fieldNames.Length; j++)
                                    {
                                        int currentFieldIndex = fieldIndices[j];
                                        int currentIndex = fieldIndices[j];
                                        // Validación adicional
                                        // 2.Validación adicional del índice
                                        if (currentIndex >= reader.FieldCount)
                                        {
                                            _logger.LogError("Índice {CurrentIndex} para campo '{FieldName}' excede los campos disponibles.", currentIndex, fieldNames[j]);
                                            allFieldsMatch = false;
                                            break;
                                        }

                                        object rawValue;
                                        try
                                        {
                                            rawValue = reader.GetValue(currentFieldIndex);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Error leyendo campo '{FieldName}' (índice {CurrentFieldIndex}) en fila {Count}", fieldNames[j], currentFieldIndex, cont);
                                            rawValue = null;
                                        }

                                        string fieldValue = rawValue?.ToString();

                                        // Comparación segura
                                        bool isMatch = exactMatch
                                            ? string.Equals(fieldValue, searchValues[j], StringComparison.OrdinalIgnoreCase)
                                            : fieldValue?.Contains(searchValues[j], StringComparison.OrdinalIgnoreCase) ?? false;

                                        if (!isMatch)
                                        {
                                            allFieldsMatch = false;
                                            break;
                                        }
                                    }

                                    // 3. Si coinciden, añadir fila
                                    if (allFieldsMatch)
                                    {
                                        try
                                        {
                                            var row = dataTable.NewRow();
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                                            }
                                            currentChunk.Add(row); // <-- ¿Falla aquí?
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Error al añadir fila #{TotalRowsScanned}", result.TotalRowsScanned);
                                            throw;
                                        }

                                        if (currentChunk.Count >= chunkSize)
                                        {
                                            await ProcessChunkAsync(dataTable, currentChunk);
                                            result.MatchingRows.AddRange(currentChunk);
                                            currentChunk.Clear();
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogCritical(ex, "Error crítico en registro #{TotalRowsScanned}", result.TotalRowsScanned);
                                    throw;
                                }
                            }
                            catch (Exception ex) when (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
                            {
                                _logger.LogWarning("Registro #{RowNumber} corrupto. Saltando...", result.TotalRowsScanned + 1);
                                File.AppendAllText("corrupt_records.log", $"Registro #{result.TotalRowsScanned + 1}: {ex.Message}\n");
                                continue;
                            }
                        }

                        // Agregar las filas restantes
                        _logger.LogDebug("Agregar las filas restantes");
                        if (currentChunk.Count > 0)
                        {
                            result.MatchingRows.AddRange(currentChunk);
                        }
                    }
                }

                progressCallback?.Invoke(100);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error crítico en registro #{TotalRowsScanned}", result.TotalRowsScanned);
                throw;
            }
        }
        private async Task ProcessChunkAsync(DataTable dataTable, List<DataRow> chunk)
        {
            await Task.Run(() =>
            {
                dataTable.BeginLoadData();
                foreach (var row in chunk)
                {
                    dataTable.Rows.Add(row);
                }
                dataTable.EndLoadData();
            });
        }
        private Type MapDbfTypeToNetType(Type dbfFieldType)
        {
            if (dbfFieldType == typeof(string)) return typeof(string);
            if (dbfFieldType == typeof(DateTime)) return typeof(DateTime);
            if (dbfFieldType == typeof(bool)) return typeof(bool);
            if (dbfFieldType == typeof(int)) return typeof(int);
            if (dbfFieldType == typeof(long)) return typeof(long);
            if (dbfFieldType == typeof(float)) return typeof(float);
            if (dbfFieldType == typeof(double)) return typeof(double);
            if (dbfFieldType == typeof(decimal)) return typeof(decimal);
            if (dbfFieldType == typeof(Guid)) return typeof(Guid);
            if (dbfFieldType == typeof(TimeSpan)) return typeof(TimeSpan);
            return typeof(string);
        }
        private Encoding GetBestEncodingForDbf()
        {
            var encodings = new int[] { 1252, 850, 28591, 437 };
            foreach (var codepage in encodings)
            {
                try
                {
                    return Encoding.GetEncoding(codepage);
                }
                catch
                {
                    continue;
                }
            }
            return Encoding.Latin1;
        }
        private Type GetNonNullableType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Nullable.GetUnderlyingType(type);
            }
            return type;
        }
    }
}
