using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using DbfDataReader;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro;

public class FoxProReportService : IFoxProReportService
{
    private readonly IConfiguracionService _configService;
    private readonly ILogger<FoxProReportService> _logger;

    public FoxProReportService(IConfiguracionService configService, ILogger<FoxProReportService> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public async Task<IEnumerable<CashCutDto>> GetDailyCashCutsAsync(DateTime date, int branchId, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        return await Task.Run(() =>
        {
            var cashCuts = new List<CashCutDto>();

            try
            {
                if (!File.Exists(config.Pos10042Path))
                {
                    _logger.LogWarning("POS10042 file not found at {Path}", config.Pos10042Path);
                    return cashCuts;
                }

                using var stream = File.OpenRead(config.Pos10042Path);
                var options = new DbfDataReaderOptions
                {
                    Encoding = Encoding.GetEncoding(28591),
                    SkipDeletedRecords = true
                };

                using var reader = new DbfDataReader.DbfDataReader(stream, options);
                
                while (reader.Read())
                {
                    try
                    {
                        var cutDate = reader.GetDateTime(reader.GetOrdinal("CFECHACOR"));

                        if (cutDate.Date == date.Date)
                        {
                            var cashRegisterName = GetCashRegisterName(
                                Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("CIDCAJA"))), config);

                            if (!string.IsNullOrEmpty(cashRegisterName))
                            {
                                // Note: This is a simplified version
                                // The actual implementation would need to parse and calculate totals
                                cashCuts.Add(new CashCutDto
                                {
                                    CashRegisterName = cashRegisterName,
                                    CutDateTime = cutDate,
                                    BranchName = $"Branch {branchId}",
                                    CashRegisterId = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("CIDCAJA"))),
                                    RawInvoices = reader.GetString(reader.GetOrdinal("CFACTURA"))?.Trim() ?? "",
                                    RawReturns = reader.GetString(reader.GetOrdinal("CDEVOLUCIO"))?.Trim() ?? ""
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error reading cash cut record");
                        continue;
                    }
                }

                _logger.LogInformation("Retrieved {Count} cash cuts for date {Date}", cashCuts.Count, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash cuts from FoxPro");
                throw;
            }

            return cashCuts;
        }, cancellationToken);
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByDateAndBranchAsync(DateTime date, int branchId, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        return await Task.Run(() =>
        {
            var documents = new List<DocumentDto>();

            try
            {
                if (!File.Exists(config.Mgw10008Path))
                {
                    _logger.LogWarning("MGW10008 file not found at {Path}", config.Mgw10008Path);
                    return documents;
                }

                using var stream = File.OpenRead(config.Mgw10008Path);
                var options = new DbfDataReaderOptions
                {
                    Encoding = Encoding.GetEncoding(28591),
                    SkipDeletedRecords = true
                };

                using var reader = new DbfDataReader.DbfDataReader(stream, options);

                while (reader.Read())
                {
                    try
                    {
                        var docDate = reader.GetDateTime(reader.GetOrdinal("CFECHA"));

                        if (docDate.Date == date.Date)
                        {
                            var documentNumber = reader.GetString(reader.GetOrdinal("CIDDOCUM02"))?.Trim() ?? "";
                            var total = reader.GetDecimal(reader.GetOrdinal("CTOTAL"));

                            documents.Add(new DocumentDto
                            {
                                DocumentNumber = documentNumber,
                                Date = docDate,
                                Total = total,
                                CustomerName = "",
                                IdDocumento = documentNumber,
                                Serie = reader.GetString(reader.GetOrdinal("CSERIEDO01"))?.Trim() ?? "",
                                Folio = reader.GetString(reader.GetOrdinal("CFOLIO"))?.Trim() ?? "",
                                CajaTexto = reader.GetString(reader.GetOrdinal("CTEXTOEX03"))?.Trim() ?? ""
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error reading document record");
                        continue;
                    }
                }

                _logger.LogInformation("Retrieved {Count} documents for date {Date}", documents.Count, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents from FoxPro");
                throw;
            }

            return documents;
        }, cancellationToken);
    }

    public async Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        return await Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrEmpty(config.Mgw10008Path))
                    return false;

                if (!File.Exists(config.Mgw10008Path))
                    return false;

                using var stream = File.OpenRead(config.Mgw10008Path);
                var options = new DbfDataReaderOptions
                {
                    Encoding = Encoding.GetEncoding(28591)
                };

                using var reader = new DbfDataReader.DbfDataReader(stream, options);
                
                _logger.LogInformation("FoxPro connection verified successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FoxPro connection verification failed");
                return false;
            }
        }, cancellationToken);
    }

    private string GetCashRegisterName(int cashRegisterId, ConfiguracionDto config)
    {
        try
        {
            if (!File.Exists(config.Pos10041Path))
                return string.Empty;

            using var stream = File.OpenRead(config.Pos10041Path);
            var options = new DbfDataReaderOptions
            {
                Encoding = Encoding.GetEncoding(28591)
            };

            using var reader = new DbfDataReader.DbfDataReader(stream, options);
            
            while (reader.Read())
            {
                var id = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("CIDCAJA")));
                if (id == cashRegisterId)
                {
                    return reader.GetString(reader.GetOrdinal("CSERIENOTA"))?.Trim() ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting cash register name for ID {CashRegisterId}", cashRegisterId);
        }

        return string.Empty;
    }

    public BranchSeriesDto GetBranchSeries(string branchName)
    {
        try
        {
            var series = Domain.ValueObjects.SeriesDocumentoSucursal.ObtenerPorNombre(branchName);
            return new BranchSeriesDto
            {
                Cliente = series.SerieCliente,
                Global = series.SerieGlobal,
                Devolucion = series.SerieDevolucion
            };
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Branch series not found for branch: {BranchName}. Returning empty series.", branchName);
            return new BranchSeriesDto();
        }
    }
    public async Task<DiagnosticoResultado> DiagnosticarArchivoAsync(string path, List<string>? expectedColumns = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resultado = new DiagnosticoResultado
            {
                RutaCompleta = path,
                NombreArchivo = Path.GetFileName(path),
                ColumnasEsperadas = expectedColumns ?? new List<string>()
            };

            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Iniciando diagnóstico de {path}");

            if (!File.Exists(path))
            {
                resultado = resultado with { ArchivoExiste = false, Exitoso = false };
                resultado.Errores.Add("El archivo no existe.");
                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Error: El archivo no existe en la ruta especificada.");
                sw.Stop();
                return resultado with { TiempoEjecucion = sw.Elapsed };
            }

            var fileInfo = new FileInfo(path);
            resultado = resultado with
            {
                ArchivoExiste = true,
                TamañoBytes = fileInfo.Length,
                FechaModificacion = fileInfo.LastWriteTime
            };
            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Archivo encontrado. Tamaño: {fileInfo.Length:N0} bytes. Modificado: {fileInfo.LastWriteTime}");

            try
            {
                using var stream = File.OpenRead(path);
                var options = new DbfDataReaderOptions { Encoding = Encoding.GetEncoding(28591) };
                
                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Intentando abrir conexión DBF...");
                using var reader = new DbfDataReader.DbfDataReader(stream, options);

                // Try to get RecordCount from header if available
                int recordCount = 0;
                if (reader.DbfTable != null)
                {
                    recordCount = (int)reader.DbfTable.Header.RecordCount;
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Cabecera leída. Registros reportados: {recordCount:N0}");
                }

                resultado = resultado with
                {
                    ConexionExitosa = true,
                    NumeroRegistros = recordCount,
                    Exitoso = true
                };
                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Conexión exitosa.");

                // Columns
                if (reader.DbfTable != null)
                {
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Leyendo estructura de columnas...");
                    foreach (var col in reader.DbfTable.Columns)
                    {
                        resultado.Columnas.Add(new ColumnaInfo
                        {
                            Nombre = col.ColumnName,
                            TipoDato = col.ColumnType.ToString(),
                            Tamaño = col.Length,
                            PermiteNulos = false
                        });
                    }
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Se encontraron {resultado.Columnas.Count} columnas.");
                }
                
                // Check expected columns
                if (expectedColumns != null && expectedColumns.Any())
                {
                     var missing = expectedColumns.Where(e => !resultado.Columnas.Any(c => c.Nombre.Equals(e, StringComparison.OrdinalIgnoreCase))).ToList();
                     if (missing.Any())
                     {
                        resultado.Advertencias.Add($"Faltan las siguientes columnas esperadas: {string.Join(", ", missing)}");
                        resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Advertencia: Faltan {missing.Count} columnas esperadas: {string.Join(", ", missing)}");
                     }
                     else
                     {
                        resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Todas las columnas esperadas ({expectedColumns.Count}) fueron encontradas.");
                     }
                }
            }
            catch (Exception ex)
            {
                resultado = resultado with { Exitoso = false, ConexionExitosa = false };
                resultado.Errores.Add(ex.Message);
                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Excepción: {ex.Message}");
                _logger.LogError(ex, "Error diagnosing file {Path}", path);
            }

            sw.Stop();
            var finalMsg = resultado.Exitoso ? "Diagnóstico finalizado exitosamente." : "Diagnóstico finalizado con errores.";
            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] {finalMsg} Tiempo: {sw.ElapsedMilliseconds}ms");
            
            return resultado with { TiempoEjecucion = sw.Elapsed };
        }, cancellationToken);
    }

    public async Task<List<Dictionary<string, object>>> ObtenerRegistrosMuestraAsync(string path, int cantidad, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var list = new List<Dictionary<string, object>>();
            if (!File.Exists(path)) return list;

            try
            {
                using var stream = File.OpenRead(path);
                var options = new DbfDataReaderOptions { Encoding = Encoding.GetEncoding(28591) };
                using var reader = new DbfDataReader.DbfDataReader(stream, options);

                int count = 0;
                while (reader.Read() && count < cantidad)
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    list.Add(row);
                    count++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading sample records from {Path}", path);
            }
            return list;
        }, cancellationToken);
    }
    public async Task<ProductDto?> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(config.Mgw10005Path) || !File.Exists(config.Mgw10005Path))
                return null;

            try
            {
                using var stream = File.OpenRead(config.Mgw10005Path);
                var options = new DbfDataReaderOptions { Encoding = Encoding.GetEncoding(28591) };
                using var reader = new DbfDataReader.DbfDataReader(stream, options);

                while (reader.Read())
                {
                    var productCode = reader.GetString(reader.GetOrdinal("CCODIGOP01"))?.Trim();
                    if (string.Equals(productCode, code, StringComparison.OrdinalIgnoreCase))
                    {
                         return new ProductDto
                         {
                             Code = productCode ?? code,
                             Name = reader.GetString(reader.GetOrdinal("CNOMBREP01"))?.Trim() ?? string.Empty,
                             BasePrice = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CPRECIO1"))),
                             TaxRate = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CIMPUESTO1")))
                         };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching product {Code}", code);
            }
            return null;
        }, cancellationToken);
    }
}
