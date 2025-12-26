using System.ComponentModel.DataAnnotations;

namespace Blanquita.Models
{
    public class Cajeras
    {
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Ingrese el numero de nomina")]
        public int NumNomina { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal")]
        public int Sucursal { get; set; }
        public bool Edo {  get; set; }
    }
}
