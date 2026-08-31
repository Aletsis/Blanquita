using System;
using System.Collections.Generic;

namespace Blanquita.Domain.Entities;

public class ConciliacionCorte : BaseEntity
{
    public int AperturaId { get; private set; }
    public string Sucursal { get; private set; } = string.Empty;
    public string Caja { get; private set; } = string.Empty;
    public string Cajero { get; private set; } = string.Empty;
    public decimal TotalRecolecciones { get; private set; }
    public decimal EfectivoEntregado { get; private set; }
    public decimal SalidasEfectivo { get; private set; }
    public decimal TotalEfectivo { get; private set; }
    public decimal Banregio { get; private set; }
    public decimal Banbajio { get; private set; }
    public decimal TotalTarjetas { get; private set; }
    public decimal Devoluciones { get; private set; }
    public decimal TotalEntregado { get; private set; }
    public decimal TotalEsperado { get; private set; }
    public decimal Diferencia { get; private set; }
    public string? TerminalesJson { get; private set; }
    public string? Usuario { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime Fecha { get; private set; }
    public DateTime? FechaModificacion { get; private set; }
    public string? ModificadoPor { get; private set; }

    public ICollection<ConciliacionSalidaEfectivo> Salidas { get; private set; } = new List<ConciliacionSalidaEfectivo>();

    // EF Core Constructor
    private ConciliacionCorte() { }

    public ConciliacionCorte(
        int aperturaId,
        string sucursal,
        string caja,
        string cajero,
        decimal totalRecolecciones,
        decimal efectivoEntregado,
        decimal salidasEfectivo,
        decimal totalEfectivo,
        decimal banregio,
        decimal banbajio,
        decimal totalTarjetas,
        decimal devoluciones,
        decimal totalEntregado,
        decimal totalEsperado,
        decimal diferencia,
        DateTime fecha,
        string? terminalesJson = null,
        string? usuario = null)
    {
        AperturaId = aperturaId;
        Sucursal = sucursal;
        Caja = caja;
        Cajero = cajero;
        TotalRecolecciones = totalRecolecciones;
        EfectivoEntregado = efectivoEntregado;
        SalidasEfectivo = salidasEfectivo;
        TotalEfectivo = totalEfectivo;
        Banregio = banregio;
        Banbajio = banbajio;
        TotalTarjetas = totalTarjetas;
        Devoluciones = devoluciones;
        TotalEntregado = totalEntregado;
        TotalEsperado = totalEsperado;
        Diferencia = diferencia;
        TerminalesJson = terminalesJson;
        Usuario = usuario;
        Fecha = fecha.Kind == DateTimeKind.Utc ? fecha : DateTime.SpecifyKind(fecha, DateTimeKind.Local).ToUniversalTime();
        FechaCreacion = DateTime.UtcNow;
    }

    public void Actualizar(
        decimal efectivoEntregado,
        decimal salidasEfectivo,
        decimal totalEfectivo,
        decimal banregio,
        decimal banbajio,
        decimal totalTarjetas,
        decimal totalEntregado,
        decimal diferencia,
        string? terminalesJson,
        string? modificadoPor)
    {
        EfectivoEntregado = efectivoEntregado;
        SalidasEfectivo = salidasEfectivo;
        TotalEfectivo = totalEfectivo;
        Banregio = banregio;
        Banbajio = banbajio;
        TotalTarjetas = totalTarjetas;
        TotalEntregado = totalEntregado;
        Diferencia = diferencia;
        TerminalesJson = terminalesJson;
        ModificadoPor = modificadoPor;
        FechaModificacion = DateTime.UtcNow;
    }
}
