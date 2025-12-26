namespace Blanquita.Models
{
    public class ReportRow
    {
        public string Fecha { get; set; } = "";
        public string Caja { get; set; } = "";
        public decimal Facturado { get; set; }
        public decimal Devolucion { get; set; }
        public decimal VentaGlobal { get; set; }
        public decimal Total { get; set; }
    }
}
