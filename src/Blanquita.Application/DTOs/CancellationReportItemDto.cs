using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blanquita.Application.DTOs;

public record CancellationReportItemDto
{
    public string IdDocumento { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    
    [DisplayName("Tipo Cancelación")]
    public string TipoCancelacion { get; init; } = "Completa"; // "Completa" o "Parcial"
    
    [DisplayName("Tipo Documento")]
    public string TipoDocumento { get; init; } = "Venta POS";
    
    public string Cliente { get; init; } = "PUBLICO GENERAL";
    public string Caja { get; init; } = string.Empty;
    public string Cajero { get; init; } = string.Empty;
    
    public decimal Neto { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
    
    [DisplayName("Partidas Canceladas")]
    public int PartidasCanceladasCount { get; init; }
    
    public List<CancellationDetailDto> Detalles { get; init; } = new();
}

public class CancellationDetailDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public double Units { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}
