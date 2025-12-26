using System;

namespace Blanquita.Models
{
    public class DocumentoMGW
    {
        public string IdDocumento { get; set; } = "";
        public string Serie { get; set; } = "";
        public string Folio { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string CajaTexto { get; set; } = "";
    }
}
