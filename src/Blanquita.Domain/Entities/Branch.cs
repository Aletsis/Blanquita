using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; private set; }
    public string SeriesCliente { get; private set; }
    public string SeriesGlobal { get; private set; }
    public string SeriesDevolucion { get; private set; }

    private Branch() { }

    public Branch(string name, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        Name = name;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
    }

    public static Branch Create(string name, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        return new Branch(name, seriesCliente, seriesGlobal, seriesDevolucion);
    }
    
    public void Update(string name, string seriesCliente, string seriesGlobal, string seriesDevolucion)
    {
        Name = name;
        SeriesCliente = seriesCliente;
        SeriesGlobal = seriesGlobal;
        SeriesDevolucion = seriesDevolucion;
    }
}
