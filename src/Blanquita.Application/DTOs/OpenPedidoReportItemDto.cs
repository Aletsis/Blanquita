using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blanquita.Application.DTOs;

public record OpenPedidoReportItemDto
{
    public string IdDocumento { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string Serie { get; init; } = string.Empty;
    public string Folio { get; init; } = string.Empty;
    
    [DisplayName("Comanda")]
    public string Comanda { get; init; } = string.Empty;
    
    [DisplayName("Sucursal")]
    public string Sucursal { get; init; } = string.Empty;
    
    public string Cliente { get; init; } = "PUBLICO GENERAL";
    public string Ruta { get; init; } = string.Empty;
    public string Repartidor { get; init; } = string.Empty;
    
    [DisplayName("Días Abierto")]
    public int DiasAbierto { get; init; }
    
    public decimal Neto { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
    
    [DisplayName("Partidas")]
    public int PartidasCount { get; init; }
    
    public List<PedidoItemDto> Detalles { get; init; } = new();
}
