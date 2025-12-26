namespace Blanquita.Models
{
    public class PrintCorteData
    {
        public int TotalM { get; set; }
        public int TotalQ { get; set; }
        public int TotalD { get; set; }
        public int TotalC { get; set; }
        public int TotalCi { get; set; }
        public int TotalV { get; set; }
        public decimal TotalTira { get; set; }
        public decimal TotalTarjetas { get; set; }
        public int GranTotal { get; set; }
        public string Caja { get; set; }
        public string Encargada { get; set; }
        public string Cajera { get; set; }
        public string Sucursal { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
