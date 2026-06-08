namespace Blanquita.Application.DTOs;

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SeriesCliente { get; set; } = "";
    public string SeriesGlobal { get; set; } = "";
    public string SeriesDevolucion { get; set; } = "";
    public string Direccion { get; set; } = string.Empty;
    public string? ConceptosSalida { get; set; }
}
