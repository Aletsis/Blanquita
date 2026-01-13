namespace Blanquita.Application.DTOs;

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SeriesCliente { get; set; } = "";
    public string SeriesGlobal { get; set; } = "";
    public string SeriesDevolucion { get; set; } = "";
}
