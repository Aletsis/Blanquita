namespace Blanquita.Models
{
    public class Reporte
    {
        public int Id { get; set; }
        public string Sucursal { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal TotalSistema { get; set; }
        public decimal TotalCorteManual { get; set; }
        public decimal Diferencia { get; set; }
        public string Notas { get; set; } = "";
        public DateTime FechaGeneracion { get; set; }
        public List<ReportRow> Detalles { get; set; } = new();
    }

}