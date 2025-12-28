namespace Blanquita.Domain.Entities;

public class Printer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsActive { get; set; } = true;
}
