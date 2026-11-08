namespace Blanquita.Application.DTOs;

public class PedidoDto
{
    /// <summary>Folio compuesto con formato "SERIE-NUMERO".</summary>
    public string Folio { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string NumeroFolio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Hora { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Comanda { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string IdDocumento { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteCodigo { get; set; } = string.Empty;
    public string Domicilio { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public List<PedidoItemDto> Items { get; set; } = new();
}
