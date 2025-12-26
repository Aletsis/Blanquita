using System.ComponentModel.DataAnnotations;

namespace Blanquita.Models
{
    public class Cajas
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "La IP de la impresora es obligatoria")]
        public string IpImpresora { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal")]
        public int Sucursal { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El puerto de impresora es obligatorio")]
        public int Port { get; set; } = 9100;
        public bool Ultima { get; set; }
    }
}
