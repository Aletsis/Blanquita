using Blanquita.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Blanquita.Interfaces
{
    public interface IFoxProService
    {
        Task<List<CorteDelDia>> GetCortesDelDiaAsync(DateTime fecha);
        Task<List<DocumentoMGW>> GetDocumentosPorFechaYSucursalAsync(DateTime fecha, BranchSeries series);
        BranchSeries GetBranchSeries(string branch);
        Task<bool> VerificarConexionAsync();

        // Métodos de diagnóstico
        Task<DiagnosticoResultado> DiagnosticarArchivoAsync(string rutaArchivo);
        Task<List<Dictionary<string, object>>> ObtenerRegistrosMuestraAsync(string rutaArchivo, int cantidad = 5);
        Dictionary<string, object> BuscarProducto(string codigo);
    }

    public class DiagnosticoResultado
    {
        public bool Exitoso { get; set; }
        public string NombreArchivo { get; set; } = "";
        public string RutaCompleta { get; set; } = "";
        public List<string> Logs { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();

        // Información del archivo
        public bool ArchivoExiste { get; set; }
        public long TamañoBytes { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Información de la base de datos
        public bool ConexionExitosa { get; set; }
        public int NumeroRegistros { get; set; }
        public List<ColumnInfo> Columnas { get; set; } = new();
        public List<string> ColumnasEsperadas { get; set; } = new();
        public List<string> ColumnasFaltantes { get; set; } = new();

        public TimeSpan TiempoEjecucion { get; set; }
    }

    public class ColumnInfo
    {
        public string Nombre { get; set; } = "";
        public string TipoDato { get; set; } = "";
        public int? Tamaño { get; set; }
        public bool PermiteNulos { get; set; }
    }
}