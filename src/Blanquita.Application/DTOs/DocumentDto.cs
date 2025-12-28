namespace Blanquita.Application.DTOs;

public record DocumentDto
{
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public decimal Total { get; init; }
    public string CustomerName { get; init; } = string.Empty;

    // FoxPro specific fields
    public string IdDocumento { get; init; } = string.Empty; // Alias for DocumentNumber (CIDDOCUM02)
    public string Serie { get; init; } = string.Empty;       // CSERIEDO01
    public string Folio { get; init; } = string.Empty;       // CFOLIO
    public string CajaTexto { get; init; } = string.Empty; // CTEXTOEX03
}
