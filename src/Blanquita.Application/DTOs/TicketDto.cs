namespace Blanquita.Application.DTOs;

public record TicketDto
{
    public string Title { get; init; } = string.Empty;
    public List<string> Lines { get; init; } = new();
    public string PrinterIp { get; init; } = string.Empty;
    public int PrinterPort { get; init; }
}
