using System;

namespace Blanquita.Application.DTOs;

public class ConciliacionCorteDto
{
    public int AperturaId { get; set; }
    public string Sucursal { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public string Cajero { get; set; } = string.Empty;
    public decimal TotalRecolecciones { get; set; }
    public decimal EfectivoEntregado { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal Banregio { get; set; }
    public decimal Banbajio { get; set; }
    public decimal TotalTarjetas { get; set; }
    public decimal Devoluciones { get; set; }
    public decimal TotalEntregado { get; set; }
    public decimal TotalEsperado { get; set; }
    public decimal Diferencia { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime FechaCreacion { get; set; }
}
