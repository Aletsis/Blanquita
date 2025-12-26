namespace Blanquita.Models
{
    public class Recolecciones
    {
        public int Id { get; set; }
        public int CantidadTotal { get; set; }
        public int Mil { get; set; }
        public int Quinientos { get; set; }
        public int Doscientos { get; set; }
        public int Cien { get; set; }
        public int Cincuenta { get; set; }
        public int Veinte { get; set; }
        public string Caja { get; set; }
        public string Cajera { get; set; }
        public string Encargada { get; set; }
        public DateTime FechaHora { get; set; }
        public int Folio {  get; set; }
        public bool Corte {  get; set; }
    }
}
