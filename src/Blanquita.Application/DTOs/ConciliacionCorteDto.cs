using System;
using System.Collections.Generic;

namespace Blanquita.Application.DTOs;

public class ConciliacionCorteDto
{
    public int Id { get; set; }
    public int AperturaId { get; set; }
    public string Sucursal { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public string Cajero { get; set; } = string.Empty;
    public decimal TotalRecolecciones { get; set; }
    public decimal EfectivoEntregado { get; set; }
    public decimal SalidasEfectivo { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal Banregio { get; set; }
    public decimal Banbajio { get; set; }
    public decimal TotalTarjetas { get; set; }
    public decimal Devoluciones { get; set; }
    public decimal TotalEntregado { get; set; }
    public decimal TotalEsperado { get; set; }
    public decimal Diferencia { get; set; }
    public string? TerminalesJson { get; set; }
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }

    public List<ConciliacionSalidaEfectivoDto> Salidas { get; set; } = new();
    public List<TerminalDetalleDto> Terminales { get; set; } = new();
}

public class ConciliacionSalidaEfectivoDto
{
    public int Id { get; set; }
    public int ConciliacionCorteId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string QuienAutoriza { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
}

public class TerminalDetalleDto
{
    public string Banco { get; set; } = string.Empty; // "Banbajio" o "Banregio"
    public string Nombre { get; set; } = string.Empty; // Ej. "Terminal 1", "Terminal Pedidos"
    public decimal Monto { get; set; }
}
