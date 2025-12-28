namespace Blanquita.Application.DTOs;

public record DiagnosticoResultado
{
    public bool Exitoso { get; init; }
    public TimeSpan TiempoEjecucion { get; init; }
    public List<string> Errores { get; init; } = new();
    public List<string> Advertencias { get; init; } = new();
    public string NombreArchivo { get; init; } = string.Empty;
    public bool ArchivoExiste { get; init; }
    public long TamañoBytes { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public bool ConexionExitosa { get; init; }
    public int NumeroRegistros { get; init; }
    public List<ColumnaInfo> Columnas { get; init; } = new();
    public List<string> ColumnasEsperadas { get; init; } = new();
    public List<string> Logs { get; init; } = new();
    public string RutaCompleta { get; init; } = string.Empty;
}

public record ColumnaInfo
{
    public string Nombre { get; init; } = string.Empty;
    public string TipoDato { get; init; } = string.Empty;
    public int? Tamaño { get; init; }
    public bool PermiteNulos { get; init; }
}
