using Blanquita.Interfaces;
using Blanquita.Models;
using Blanquita.Services.Parsing;
using DbfDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blanquita.Services
{
    public class FoxProService : IFoxProService
    {
        private readonly AppConfiguration _config;
        private readonly ILogger<FoxProService> _logger;
        private readonly IDbfStringParser _dbfParser;

        public FoxProService(AppConfiguration config, ILogger<FoxProService> logger, IDbfStringParser dbfParser)
        {
            _config = config;
            _logger = logger;
            _dbfParser = dbfParser;
        }

        public async Task<List<ReportRow>> GenerarReportDataAsync(string sucursal, DateTime fecha)
        {
            return await Task.Run(() => GenerarReportData(sucursal, fecha));
        }

        private List<ReportRow> GenerarReportData(string sucursal, DateTime fecha)
        {
            var result = new List<ReportRow>();
            var series = GetBranchSeries(sucursal);

            _logger.LogDebug("=== Iniciando generación de reporte ===");
            _logger.LogDebug("Sucursal: {Sucursal}", sucursal);
            _logger.LogDebug("Fecha: {Fecha:dd/MM/yyyy}", fecha);
            _logger.LogDebug("Series - Cliente: {SerieCliente}, Global: {SerieGlobal}, Devolucion: {SerieDevolucion}", series.Cliente, series.Global, series.Devolucion);

            if (string.IsNullOrEmpty(_config.Pos10041Path) ||
                string.IsNullOrEmpty(_config.Pos10042Path) ||
                string.IsNullOrEmpty(_config.Mgw10008Path))
            {
                _logger.LogError("Configuración incompleta");
                throw new Exception("Configuración no establecida. Por favor configure las rutas en Configuración.");
            }

            try
            {
                // Paso 1: Obtener todos los cortes de la fecha especificada
                _logger.LogDebug("Paso 1: Buscando cortes en fecha {Fecha:dd/MM/yyyy}...", fecha);
                var cortes = ObtenerCortesDelDia(fecha);
                _logger.LogDebug("Total de cortes encontrados: {Count}", cortes.Count);

                if (!cortes.Any())
                {
                    _logger.LogDebug("No hay cortes para esta fecha");
                    return result;
                }

                // Paso 2: Obtener todos los documentos de la fecha y sucursal
                _logger.LogDebug("Paso 2: Obteniendo documentos de la fecha para series de {Sucursal}...", sucursal);
                var documentos = ObtenerDocumentosPorFechaYSucursal(fecha, series);
                _logger.LogDebug("Total de documentos encontrados: {Count}", documentos.Count);

                // Paso 3: Procesar cada corte
                foreach (var corte in cortes)
                {
                    _logger.LogDebug("Procesando corte - Caja ID: {IdCaja}, Serie: '{SerieCaja}'", corte.IdCaja, corte.SerieCaja);

                    decimal facturado = 0;
                    decimal ventaGlobal = 0;
                    decimal devolucion = 0;

                    // Procesar facturas (Cliente) - usar CTEXTOEX03 para identificar la caja
                    var docsCliente = documentos.Where(d =>
                        d.Serie == series.Cliente &&
                        d.CajaTexto.Equals(corte.SerieCaja, StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (var doc in docsCliente)
                    {
                        facturado += doc.Total;
                        _logger.LogDebug("  + Factura Cliente: {Serie}-{Folio} (Caja: {CajaTexto}) = {Total:C2}", doc.Serie, doc.Folio, doc.CajaTexto, doc.Total);
                    }

                    // Procesar facturas (Global) - usar campo CFACTURA del corte
                    var docsFactGlobal = _dbfParser.ParsearDocumentos(corte.Facturas);
                    foreach (var doc in docsFactGlobal)
                    {
                        var docEncontrado = documentos.FirstOrDefault(d =>
                            d.IdDocumento == doc.IdDocumento &&
                            d.Serie == doc.Serie &&
                            d.Folio == doc.Folio &&
                            d.Serie == series.Global);

                        if (docEncontrado != null)
                        {
                            ventaGlobal += docEncontrado.Total;
                            _logger.LogDebug("  + Venta Global: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                        }
                    }

                    // Procesar devoluciones - usar campo CDEVOLUCIO del corte
                    var docsDev = _dbfParser.ParsearDocumentos(corte.Devoluciones);
                    foreach (var doc in docsDev)
                    {
                        var docEncontrado = documentos.FirstOrDefault(d =>
                            d.IdDocumento == doc.IdDocumento &&
                            d.Serie == doc.Serie &&
                            d.Folio == doc.Folio &&
                            d.Serie == series.Devolucion);

                        if (docEncontrado != null)
                        {
                            devolucion += docEncontrado.Total;
                            _logger.LogDebug("  - Devolución: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                        }
                    }

                    decimal total = facturado + ventaGlobal - devolucion;
                    _logger.LogDebug("  Totales caja '{SerieCaja}': Fact={Facturado:C2}, VG={VentaGlobal:C2}, Dev={Devolucion:C2}, Total={Total:C2}", corte.SerieCaja, facturado, ventaGlobal, devolucion, total);

                    if (facturado > 0 || ventaGlobal > 0 || devolucion > 0)
                    {
                        result.Add(new ReportRow
                        {
                            Fecha = fecha.ToString("dd/MM/yyyy"),
                            Caja = corte.SerieCaja,
                            Facturado = facturado,
                            Devolucion = devolucion,
                            VentaGlobal = ventaGlobal,
                            Total = total
                        });
                        _logger.LogDebug("  ✓ Registro agregado al reporte");
                    }
                }

                _logger.LogDebug("=== Fin de generación ===");
                _logger.LogDebug("Total de registros en el reporte: {Count}", result.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción durante la generación");
                throw new Exception($"Error al generar reporte: {ex.Message}", ex);
            }

            return result;
        }

        private List<CorteDelDia> ObtenerCortesDelDia(DateTime fecha)
        {
            var cortes = new List<CorteDelDia>();
            int totalRegistros = 0;
            int cortesEnFecha = 0;

            try
            {
                using (var stream = File.OpenRead(_config.Pos10042Path))
                {
                    var options = new DbfDataReader.DbfDataReaderOptions
                    {
                        Encoding = Encoding.GetEncoding(28591),
                        SkipDeletedRecords = true
                    };

                    using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                    {
                        while (reader.Read())
                        {
                            totalRegistros++;
                            try
                            {
                                var fechaCorte = reader.GetDateTime(reader.GetOrdinal("CFECHACOR"));

                                if (fechaCorte.Date == fecha.Date)
                                {
                                    cortesEnFecha++;
                                    var idCaja = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("CIDCAJA")));
                                    var facturas = reader.GetString(reader.GetOrdinal("CFACTURA"))?.Trim() ?? "";
                                    var devoluciones = reader.GetString(reader.GetOrdinal("CDEVOLUCIO"))?.Trim() ?? "";

                                    // Obtener nombre de la caja desde POS10041
                                    var serieCaja = ObtenerNombreCaja(idCaja);

                                    if (!string.IsNullOrEmpty(serieCaja))
                                    {
                                        cortes.Add(new CorteDelDia
                                        {
                                            IdCaja = idCaja,
                                            SerieCaja = serieCaja,
                                            Facturas = facturas,
                                            Devoluciones = devoluciones
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug("Error al leer corte: {Message}", ex.Message);
                                continue;
                            }
                        }
                    }
                }

                _logger.LogDebug("Estadísticas POS10042:");
                _logger.LogDebug("  Total registros: {TotalRegistros}", totalRegistros);
                _logger.LogDebug("  Cortes en fecha {Fecha:dd/MM/yyyy}: {CortesEnFecha}", fecha, cortesEnFecha);
                _logger.LogDebug("  Cortes con caja válida: {Count}", cortes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cortes");
                throw;
            }

            return cortes;
        }

        private string ObtenerNombreCaja(int idCaja)
        {
            try
            {
                using (var stream = File.OpenRead(_config.Pos10041Path))
                {
                    var options = new DbfDataReader.DbfDataReaderOptions
                    {
                        Encoding = Encoding.GetEncoding(28591)
                    };

                    using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                    {
                        while (reader.Read())
                        {
                            var id = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("CIDCAJA")));
                            if (id == idCaja)
                            {
                                return reader.GetString(reader.GetOrdinal("CSERIENOTA"))?.Trim() ?? "";
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si hay error, retornar vacío
            }

            return "";
        }

        private List<DocumentoMGW> ObtenerDocumentosPorFechaYSucursal(DateTime fecha, BranchSeries series)
        {
            var documentos = new List<DocumentoMGW>();
            var seriesBuscadas = new[] { series.Cliente, series.Global, series.Devolucion };

            _logger.LogDebug("  Buscando documentos con series: {Series}", string.Join(", ", seriesBuscadas));

            try
            {
                using (var stream = File.OpenRead(_config.Mgw10008Path))
                {
                    var options = new DbfDataReader.DbfDataReaderOptions
                    {
                        Encoding = Encoding.GetEncoding(28591),
                        SkipDeletedRecords = true
                    };

                    using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                    {
                        int totalRevisados = 0;
                        int encontrados = 0;
                        int erroresFecha = 0;
                        int erroresSerie = 0;
                        int erroresOtros = 0;
                        int conFechaCorrecta = 0;

                        // Obtener ordinales una sola vez
                        int cfechaOrd = reader.GetOrdinal("CFECHA");
                        int cserieOrd = reader.GetOrdinal("CSERIEDO01");
                        int cidDocOrd = reader.GetOrdinal("CIDDOCUM02");
                        int cfolioOrd = reader.GetOrdinal("CFOLIO");
                        int ctotalOrd = reader.GetOrdinal("CTOTAL");
                        int ctextoex03Ord = reader.GetOrdinal("CTEXTOEX03");

                        while (reader.Read())
                        {
                            totalRevisados++;

                            try
                            {
                                // Intentar leer fecha con manejo de nulos
                                DateTime fechaDoc;
                                if (reader.IsDBNull(cfechaOrd))
                                {
                                    erroresFecha++;
                                    continue;
                                }

                                try
                                {
                                    fechaDoc = reader.GetDateTime(cfechaOrd);
                                }
                                catch
                                {
                                    erroresFecha++;
                                    continue;
                                }

                                // Filtrar por fecha
                                if (fechaDoc.Date != fecha.Date)
                                    continue;

                                conFechaCorrecta++;

                                // Leer serie con manejo de tipos flexibles
                                string serie = "";
                                if (!reader.IsDBNull(cserieOrd))
                                {
                                    try
                                    {
                                        serie = reader.GetString(cserieOrd)?.Trim() ?? "";
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            var valor = reader.GetValue(cserieOrd);
                                            serie = valor?.ToString()?.Trim() ?? "";
                                        }
                                        catch
                                        {
                                            erroresSerie++;
                                            if (erroresSerie <= 5)
                                                _logger.LogDebug("    Registro #{TotalRevisados}: error al leer serie", totalRevisados);
                                            continue;
                                        }
                                    }
                                }

                                if (conFechaCorrecta <= 10)
                                {
                                    _logger.LogDebug("    Registro con fecha correcta #{ConFechaCorrecta}: Serie='{Serie}', Fecha={FechaDoc:dd/MM/yyyy}", conFechaCorrecta, serie, fechaDoc);
                                }

                                // Filtrar por serie
                                if (string.IsNullOrEmpty(serie) || !seriesBuscadas.Contains(serie, StringComparer.OrdinalIgnoreCase))
                                    continue;

                                // Leer el resto de campos
                                string idDocumento = "";
                                string folio = "";
                                decimal total = 0;
                                string cajaTexto = "";

                                if (!reader.IsDBNull(cidDocOrd))
                                {
                                    try
                                    {
                                        idDocumento = reader.GetString(cidDocOrd)?.Trim() ?? "";
                                    }
                                    catch
                                    {
                                        var valor = reader.GetValue(cidDocOrd);
                                        idDocumento = valor?.ToString()?.Trim() ?? "";
                                    }
                                }

                                if (!reader.IsDBNull(cfolioOrd))
                                {
                                    try
                                    {
                                        folio = reader.GetString(cfolioOrd)?.Trim() ?? "";
                                    }
                                    catch
                                    {
                                        var valor = reader.GetValue(cfolioOrd);
                                        folio = valor?.ToString()?.Trim() ?? "";
                                    }
                                }

                                if (!reader.IsDBNull(ctotalOrd))
                                {
                                    try
                                    {
                                        total = reader.GetDecimal(ctotalOrd);
                                    }
                                    catch
                                    {
                                        var valorDouble = reader.GetDouble(ctotalOrd);
                                        total = Convert.ToDecimal(valorDouble);
                                    }
                                }

                                // Leer CTEXTOEX03 para identificar la caja
                                if (!reader.IsDBNull(ctextoex03Ord))
                                {
                                    try
                                    {
                                        cajaTexto = reader.GetString(ctextoex03Ord)?.Trim() ?? "";
                                    }
                                    catch
                                    {
                                        var valor = reader.GetValue(ctextoex03Ord);
                                        cajaTexto = valor?.ToString()?.Trim() ?? "";
                                    }
                                }

                                documentos.Add(new DocumentoMGW
                                {
                                    IdDocumento = idDocumento,
                                    Serie = serie,
                                    Folio = folio,
                                    Fecha = fechaDoc,
                                    Total = total,
                                    CajaTexto = cajaTexto
                                });

                                encontrados++;

                                if (encontrados <= 10)
                                {
                                    _logger.LogDebug("    ✓ Doc #{Encontrados}: ID='{IdDocumento}', Serie='{Serie}', Folio='{Folio}', Caja='{CajaTexto}', Total={Total:C2}", encontrados, idDocumento, serie, folio, cajaTexto, total);
                                }
                            }
                            catch (Exception ex)
                            {
                                erroresOtros++;
                                if (erroresOtros <= 5)
                                {
                                    _logger.LogDebug("    Error general en registro #{TotalRevisados}: {Message}", totalRevisados, ex.Message);
                                }
                                continue;
                            }
                        }

                        _logger.LogDebug("  Total revisados: {TotalRevisados}", totalRevisados);
                        _logger.LogDebug("  Con fecha {Fecha:dd/MM/yyyy}: {ConFechaCorrecta}", fecha, conFechaCorrecta);
                        _logger.LogDebug("  Encontrados con fecha y serie correcta: {Encontrados}", encontrados);
                        _logger.LogDebug("  Errores - Fecha: {ErroresFecha}, Serie: {ErroresSerie}, Otros: {ErroresOtros}", erroresFecha, erroresSerie, erroresOtros);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos");
                throw;
            }

            return documentos;
        }



        private BranchSeries GetBranchSeries(string branch)
        {
            return branch switch
            {
                "Himno" => new BranchSeries { Cliente = "COH", Global = "FGIH", Devolucion = "DFCH" },
                "Pozos" => new BranchSeries { Cliente = "COP", Global = "FGIP", Devolucion = "DFCP" },
                "Soledad" => new BranchSeries { Cliente = "COS", Global = "FGIS", Devolucion = "DFCS" },
                "Saucito" => new BranchSeries { Cliente = "COFS", Global = "FGIFS", Devolucion = "DFCFS" },
                "Chapultepec" => new BranchSeries { Cliente = "COX", Global = "FXIS", Devolucion = "DFCX" },
                _ => new BranchSeries()
            };
        }

        public Dictionary<string, object> BuscarProducto(string codigo)
        {
            try
            {
                using (var dbfStream = new FileStream(_config.Mgw10005Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var dbfTable = new DbfTable(dbfStream, Encoding.GetEncoding(28591)))
                {
                    var dbfRecord = new DbfRecord(dbfTable);
                    int idxCodigo = -1, idxNombre = -1, idxPrecio = -1, idxImpuesto = -1;
                    for (int i = 0; i < dbfTable.Columns.Count; i++)
                    {
                        string nombreColumna = dbfTable.Columns[i].ColumnName;
                        if (nombreColumna.Equals("CCODIGOP01", StringComparison.OrdinalIgnoreCase)) idxCodigo = i;
                        else if (nombreColumna.Equals("CNOMBREP01", StringComparison.OrdinalIgnoreCase)) idxNombre = i;
                        else if (nombreColumna.Equals("CPRECIO1", StringComparison.OrdinalIgnoreCase)) idxPrecio = i;
                        else if (nombreColumna.Equals("CIMPUESTO1", StringComparison.OrdinalIgnoreCase)) idxImpuesto = i;
                    }
                    if (idxCodigo == -1) throw new ArgumentException("Campo CCODIGOP01 no encontradp en la tabla");
                    while (dbfRecord.Read(dbfStream))
                    {
                        var valorCodigo = dbfRecord.GetValue(idxCodigo);
                        if (ValoresIguales(valorCodigo, codigo))
                        {
                            var registro = new Dictionary<string, object>();
                            if (idxNombre != -1) registro["CNOMBREP01"] = dbfRecord.GetValue(idxNombre);
                            if (idxPrecio != -1) registro["CPRECIO1"] = dbfRecord.GetValue(idxPrecio);
                            if (idxImpuesto != -1) registro["CIMPUESTO1"] = dbfRecord.GetValue(idxImpuesto);
                            return registro;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar registro");
                throw;
            }
            return null;
        }
        private bool ValoresIguales(object valor1, object valor2)
        {
            if (valor1 == null && valor2 == null) return true;
            if (valor1 == null || valor2 == null) return false;
            if (valor1 is string str1 && valor2 is string str2)
            {
                return str1.Trim().Equals(str2.Trim(),StringComparison.OrdinalIgnoreCase);
            }
            return valor1.Equals(valor2);
        }

        public async Task<bool> VerificarConexionAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(_config.Mgw10008Path))
                        return false;

                    if (!File.Exists(_config.Mgw10008Path))
                        return false;

                    using (var stream = File.OpenRead(_config.Mgw10008Path))
                    {
                        var options = new DbfDataReader.DbfDataReaderOptions
                        {
                            Encoding = Encoding.GetEncoding(28591)
                        };

                        using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<DiagnosticoResultado> DiagnosticarArchivoAsync(string rutaArchivo)
        {
            return await Task.Run(() =>
            {
                var resultado = new DiagnosticoResultado
                {
                    RutaCompleta = rutaArchivo,
                    NombreArchivo = Path.GetFileName(rutaArchivo)
                };

                var inicio = DateTime.Now;

                try
                {
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Iniciando diagnóstico de {resultado.NombreArchivo}");

                    // 1. Verificar existencia del archivo
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Verificando existencia del archivo...");
                    resultado.ArchivoExiste = File.Exists(rutaArchivo);

                    if (!resultado.ArchivoExiste)
                    {
                        resultado.Errores.Add("El archivo no existe en la ruta especificada");
                        resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✗ El archivo no existe");
                        resultado.Exitoso = false;
                        resultado.TiempoEjecucion = DateTime.Now - inicio;
                        return resultado;
                    }

                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Archivo encontrado");

                    // 2. Obtener información del archivo
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Obteniendo información del archivo...");
                    var fileInfo = new FileInfo(rutaArchivo);
                    resultado.TamañoBytes = fileInfo.Length;
                    resultado.FechaModificacion = fileInfo.LastWriteTime;
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Tamaño: {FormatBytes(resultado.TamañoBytes)}");
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Última modificación: {resultado.FechaModificacion:dd/MM/yyyy HH:mm:ss}");

                    // 3. Verificar extensión
                    if (!rutaArchivo.EndsWith(".dbf", StringComparison.OrdinalIgnoreCase))
                    {
                        resultado.Advertencias.Add("El archivo no tiene extensión .dbf");
                        resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ⚠ Advertencia: Extensión inesperada");
                    }

                    // 4. Intentar abrir con DbfDataReader
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Intentando abrir el archivo DBF...");

                    using (var stream = File.OpenRead(rutaArchivo))
                    {
                        var options = new DbfDataReader.DbfDataReaderOptions
                        {
                            Encoding = Encoding.GetEncoding(28591)
                        };

                        using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                        {
                            resultado.ConexionExitosa = true;
                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Archivo abierto exitosamente");

                            // 5. Obtener estructura de columnas
                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Obteniendo estructura de columnas...");

                            var columns = reader.GetColumnSchema();
                            foreach (var column in columns)
                            {
                                resultado.Columnas.Add(new ColumnInfo
                                {
                                    Nombre = column.ColumnName,
                                    TipoDato = column.DataTypeName ?? column.DataType?.Name ?? "Unknown",
                                    Tamaño = column.ColumnSize,
                                    PermiteNulos = column.AllowDBNull ?? true
                                });
                            }

                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Se encontraron {resultado.Columnas.Count} columnas");

                            // 6. Validar columnas esperadas
                            resultado.ColumnasEsperadas = ObtenerColumnasEsperadas(resultado.NombreArchivo);
                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Validando columnas esperadas...");

                            foreach (var columnaEsperada in resultado.ColumnasEsperadas)
                            {
                                if (!resultado.Columnas.Any(c => c.Nombre.Equals(columnaEsperada, StringComparison.OrdinalIgnoreCase)))
                                {
                                    resultado.ColumnasFaltantes.Add(columnaEsperada);
                                }
                            }

                            if (resultado.ColumnasFaltantes.Any())
                            {
                                resultado.Errores.Add($"Faltan columnas esperadas: {string.Join(", ", resultado.ColumnasFaltantes)}");
                                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✗ Columnas faltantes: {string.Join(", ", resultado.ColumnasFaltantes)}");
                            }
                            else
                            {
                                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Todas las columnas esperadas están presentes");
                            }

                            // 7. Contar registros
                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Contando registros...");
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                            }
                            resultado.NumeroRegistros = count;
                            resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Total de registros: {resultado.NumeroRegistros:N0}");

                            if (resultado.NumeroRegistros == 0)
                            {
                                resultado.Advertencias.Add("La tabla no contiene registros");
                                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ⚠ Advertencia: Tabla vacía");
                            }
                            else
                            {
                                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ Lectura de datos exitosa");
                            }
                        }
                    }

                    resultado.Exitoso = !resultado.Errores.Any();
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Diagnóstico completado: {(resultado.Exitoso ? "EXITOSO" : "CON ERRORES")}");
                }
                catch (Exception ex)
                {
                    resultado.Exitoso = false;
                    resultado.Errores.Add($"Error durante el diagnóstico: {ex.Message}");
                    resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ✗ Error crítico: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Detalles: {ex.InnerException.Message}");
                    }
                }

                resultado.TiempoEjecucion = DateTime.Now - inicio;
                resultado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Tiempo total: {resultado.TiempoEjecucion.TotalSeconds:F2}s");

                return resultado;
            });
        }

        public async Task<List<Dictionary<string, object>>> ObtenerRegistrosMuestraAsync(string rutaArchivo, int cantidad = 5)
        {
            return await Task.Run(() =>
            {
                var registros = new List<Dictionary<string, object>>();

                try
                {
                    if (!File.Exists(rutaArchivo))
                        return registros;

                    using (var stream = File.OpenRead(rutaArchivo))
                    {
                        var options = new DbfDataReader.DbfDataReaderOptions
                        {
                            Encoding = Encoding.GetEncoding(28591)
                        };

                        using (var reader = new DbfDataReader.DbfDataReader(stream, options))
                        {
                            int count = 0;

                            while (reader.Read() && count < cantidad)
                            {
                                var registro = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var nombreColumna = reader.GetName(i);
                                    object valor;

                                    if (reader.IsDBNull(i))
                                    {
                                        valor = "(NULL)";
                                    }
                                    else
                                    {
                                        valor = reader.GetValue(i);
                                    }

                                    registro[nombreColumna] = valor;
                                }

                                registros.Add(registro);
                                count++;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignorar errores y devolver lista vacía
                }

                return registros;
            });
        }

        private List<string> ObtenerColumnasEsperadas(string nombreArchivo)
        {
            var nombre = nombreArchivo.ToUpper();

            if (nombre.Contains("POS10041"))
            {
                return new List<string> { "CIDCAJA", "CSERIENOTA" };
            }
            else if (nombre.Contains("POS10042"))
            {
                return new List<string> { "CIDCAJA", "CFECHACOR", "CFACTURA", "CDEVOLUCIO" };
            }
            else if (nombre.Contains("MGW10008"))
            {
                return new List<string> { "CIDDOCUM02", "CSERIEDO01", "CFOLIO", "CFECHA", "CNETO", "COBSERVA01" };
            }

            return new List<string>();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private class CorteDelDia
        {
            public int IdCaja { get; set; }
            public string SerieCaja { get; set; } = "";
            public string Facturas { get; set; } = "";
            public string Devoluciones { get; set; } = "";
        }

        private class DocumentoMGW
        {
            public string IdDocumento { get; set; } = "";
            public string Serie { get; set; } = "";
            public string Folio { get; set; } = "";
            public DateTime Fecha { get; set; }
            public decimal Total { get; set; }
            public string CajaTexto { get; set; } = "";
        }


    }
}