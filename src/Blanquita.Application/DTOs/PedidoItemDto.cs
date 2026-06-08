namespace Blanquita.Application.DTOs;

public class PedidoItemDto
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Impuesto { get; set; }
    public decimal PorcentajeImpuesto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total => Subtotal + Impuesto;
}
