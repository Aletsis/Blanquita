using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blanquita.Application.DTOs;

public record ReturnReportItemDto
{
    public string IdDocumento { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    
    [DisplayName("Tipo")]
    public string Tipo { get; init; } = "Completa"; // "Completa" o "Parcial"
    
    [DisplayName("Referencia / Nota")]
    public string Referencia { get; init; } = string.Empty;
    
    public decimal Neto { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
    
    [DisplayName("Venta Original")]
    public decimal? VentaOriginalTotal { get; init; }
    
    public int PartidasCount { get; init; }
    public List<ReturnDetailDto> Detalles { get; init; } = new();
}

