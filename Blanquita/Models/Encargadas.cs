using System.ComponentModel.DataAnnotations;

namespace Blanquita.Models
{
    public class Encargadas
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal")]
        public int Sucursal { get; set; }
    }
}
