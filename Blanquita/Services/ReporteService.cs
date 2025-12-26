using Blanquita.Interfaces;
using Blanquita.Models;
using System.Text.Json;

namespace Blanquita.Services
{
    public class ReporteService : IReporteService
    {
        private readonly string _rutaHistorico;
        private List<Reporte> _reportesCache;

        public ReporteService()
        {
            _rutaHistorico = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "BillingReportSystem",
                "Historico"
            );

            if (!Directory.Exists(_rutaHistorico))
                Directory.CreateDirectory(_rutaHistorico);

            _reportesCache = new List<Reporte>();
            CargarReportes();
        }

        public async Task GuardarReporteAsync(Reporte reporte)
        {
            await Task.Run(() =>
            {
                // Generar ID único
                reporte.Id = _reportesCache.Any() ? _reportesCache.Max(r => r.Id) + 1 : 1;

                // Agregar a la cache
                _reportesCache.Add(reporte);

                // Guardar en archivo individual
                var nombreArchivo = $"Reporte_{reporte.Id}_{reporte.Sucursal}_{reporte.Fecha:yyyyMMdd}.json";
                var rutaArchivo = Path.Combine(_rutaHistorico, nombreArchivo);

                var json = JsonSerializer.Serialize(reporte, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(rutaArchivo, json);

                // Actualizar índice
                ActualizarIndice();
            });
        }

        public async Task<List<Reporte>> ObtenerReportesAsync()
        {
            return await Task.FromResult(_reportesCache.OrderByDescending(r => r.FechaGeneracion).ToList());
        }

        public async Task<Reporte?> ObtenerReportePorIdAsync(int id)
        {
            return await Task.FromResult(_reportesCache.FirstOrDefault(r => r.Id == id));
        }

        public async Task EliminarReporteAsync(int id)
        {
            await Task.Run(() =>
            {
                var reporte = _reportesCache.FirstOrDefault(r => r.Id == id);
                if (reporte != null)
                {
                    _reportesCache.Remove(reporte);

                    // Eliminar archivo
                    var nombreArchivo = $"Reporte_{reporte.Id}_{reporte.Sucursal}_{reporte.Fecha:yyyyMMdd}.json";
                    var rutaArchivo = Path.Combine(_rutaHistorico, nombreArchivo);

                    if (File.Exists(rutaArchivo))
                        File.Delete(rutaArchivo);

                    // Actualizar índice
                    ActualizarIndice();
                }
            });
        }

        public async Task<List<Reporte>> BuscarReportesAsync(string? sucursal = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            return await Task.Run(() =>
            {
                var query = _reportesCache.AsEnumerable();

                if (!string.IsNullOrEmpty(sucursal))
                {
                    query = query.Where(r => r.Sucursal.Equals(sucursal, StringComparison.OrdinalIgnoreCase));
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(r => r.Fecha >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(r => r.Fecha <= fechaFin.Value);
                }

                return query.OrderByDescending(r => r.FechaGeneracion).ToList();
            });
        }

        private void CargarReportes()
        {
            try
            {
                var archivos = Directory.GetFiles(_rutaHistorico, "Reporte_*.json");

                foreach (var archivo in archivos)
                {
                    try
                    {
                        var json = File.ReadAllText(archivo);
                        var reporte = JsonSerializer.Deserialize<Reporte>(json);

                        if (reporte != null)
                        {
                            _reportesCache.Add(reporte);
                        }
                    }
                    catch
                    {
                        // Ignorar archivos corruptos
                    }
                }
            }
            catch
            {
                // Si hay error al cargar, iniciar con cache vacía
                _reportesCache = new List<Reporte>();
            }
        }

        private void ActualizarIndice()
        {
            try
            {
                var indice = _reportesCache.Select(r => new
                {
                    r.Id,
                    r.Sucursal,
                    Fecha = r.Fecha.ToString("yyyy-MM-dd"),
                    FechaGeneracion = r.FechaGeneracion.ToString("yyyy-MM-dd HH:mm:ss"),
                    r.TotalSistema,
                    r.TotalCorteManual,
                    r.Diferencia
                }).ToList();

                var rutaIndice = Path.Combine(_rutaHistorico, "indice.json");
                var json = JsonSerializer.Serialize(indice, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(rutaIndice, json);
            }
            catch
            {
                // Ignorar errores al actualizar índice
            }
        }
        public async Task ActualizarReporteAsync(Reporte reporte)
        {
            await Task.Run(() =>
            {
                var nombreArchivo = $"Reporte_{reporte.Id}_{reporte.Sucursal}_{reporte.Fecha:yyyyMMdd}.json";
                var rutaArchivo = Path.Combine(_rutaHistorico, nombreArchivo);

                var json = JsonSerializer.Serialize(reporte, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(rutaArchivo, json);

                // Actualizar en caché
                var reporteExistente = _reportesCache.FirstOrDefault(r => r.Id == reporte.Id);
                if (reporteExistente != null)
                {
                    _reportesCache.Remove(reporteExistente);
                    _reportesCache.Add(reporte);
                }

                ActualizarIndice();
            });
        }
    }
}