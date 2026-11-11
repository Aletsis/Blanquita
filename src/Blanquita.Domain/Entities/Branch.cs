using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string SeriesCliente { get; private set; } = string.Empty;
    public string SeriesGlobal { get; private set; } = string.Empty;
    public string SeriesDevolucion { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string? ConceptosSalida { get; private set; }
    public bool PermiteSalidasEfectivo { get; private set; }

#pragma warning disable CS8618 // EF Core asigna las propiedades al materializar
    private Branch() { }
#pragma warning restore CS8618

    public Branch(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion, string direccion, string? conceptosSalida, bool permiteSalidasEfectivo = false)
    {
        Name = name;
        Code = code;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
        Direccion = direccion;
        ConceptosSalida = conceptosSalida;
        PermiteSalidasEfectivo = permiteSalidasEfectivo;
    }

    public static Branch Create(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion, string direccion, string? conceptosSalida, bool permiteSalidasEfectivo = false)
    {
        return new Branch(name, code, seriesCliente, seriesGlobal, seriesDevolucion, direccion, conceptosSalida, permiteSalidasEfectivo);
    }
    
    public void Update(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion, string direccion, string? conceptosSalida, bool permiteSalidasEfectivo = false)
    {
        Name = name;
        Code = code;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
        Direccion = direccion;
        ConceptosSalida = conceptosSalida;
        PermiteSalidasEfectivo = permiteSalidasEfectivo;
    }
}
