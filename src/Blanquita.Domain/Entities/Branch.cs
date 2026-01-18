using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string SeriesCliente { get; private set; }
    public string SeriesGlobal { get; private set; }
    public string SeriesDevolucion { get; private set; }

    private Branch() { }

    public Branch(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        Name = name;
        Code = code;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
    }

    public static Branch Create(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        return new Branch(name, code, seriesCliente, seriesGlobal, seriesDevolucion);
    }
    
    public void Update(string name, string code, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        Name = name;
        Code = code;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
    }
}
