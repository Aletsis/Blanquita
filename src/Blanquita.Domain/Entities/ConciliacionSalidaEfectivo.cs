using System;

namespace Blanquita.Domain.Entities;

public class ConciliacionSalidaEfectivo : BaseEntity
{
    public int ConciliacionCorteId { get; private set; }
    public decimal Monto { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public string QuienAutoriza { get; private set; } = string.Empty;
    public DateTime FechaCreacion { get; private set; }
    public string UsuarioCreacion { get; private set; } = string.Empty;

    // Navigation property
    public ConciliacionCorte? ConciliacionCorte { get; private set; }

    private ConciliacionSalidaEfectivo() { }

    public ConciliacionSalidaEfectivo(
        decimal monto,
        string motivo,
        string quienAutoriza,
        string usuarioCreacion)
    {
        Monto = monto;
        Motivo = motivo ?? string.Empty;
        QuienAutoriza = quienAutoriza ?? string.Empty;
        UsuarioCreacion = usuarioCreacion ?? string.Empty;
        FechaCreacion = DateTime.UtcNow;
    }
}
