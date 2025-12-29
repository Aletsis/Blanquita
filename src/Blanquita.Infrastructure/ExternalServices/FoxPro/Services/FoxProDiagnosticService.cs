using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using DbfDataReader;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Services;

/// <summary>
/// Servicio de diagnóstico para archivos FoxPro/DBF.
/// </summary>
public class FoxProDiagnosticService : IFoxProDiagnosticService
{
    private readonly IConfiguracionService _configService;
    private readonly ILogger<FoxProDiagnosticService> _logger;

    public FoxProDiagnosticService(
        IConfiguracionService configService,
        ILogger<FoxProDiagnosticService> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public Task<DiagnosticoResultado> DiagnoseFileAsync(
        string path, 
        List<string>? expectedColumns = null, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
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

    public Task<List<Dictionary<string, object>>> GetSampleRecordsAsync(
        string path, 
        int count, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var list = new List<Dictionary<string, object>>();
            if (!File.Exists(path)) return list;

            try
            {
                using var stream = File.OpenRead(path);
                var options = new DbfDataReaderOptions { Encoding = Encoding.GetEncoding(28591) };
                using var reader = new DbfDataReader.DbfDataReader(stream, options);

                int recordCount = 0;
                while (reader.Read() && recordCount < count)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    list.Add(row);
                    recordCount++;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Lectura de registros de muestra cancelada");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading sample records from {Path}", path);
                throw new FoxProDataReadException($"Error al leer registros de muestra de {path}", path, ex);
            }
            return list;
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
                {
                    _logger.LogWarning("Ruta MGW10008 no configurada");
                    return false;
                }

                if (!File.Exists(config.Mgw10008Path))
                {
                    _logger.LogWarning("Archivo MGW10008 no encontrado: {Path}", config.Mgw10008Path);
                    return false;
                }

                using var reader = DbfReaderFactory.CreateReader(config.Mgw10008Path);
                
                _logger.LogInformation("Conexión FoxPro verificada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Verificación de conexión FoxPro falló");
                return false;
            }
        }, cancellationToken);
    }
}
