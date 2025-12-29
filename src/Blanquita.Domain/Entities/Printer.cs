namespace Blanquita.Domain.Entities;

public class Printer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsActive { get; set; } = true;
    private int _dpi = 203;

    public int Dpi
    {
        get => _dpi;
        set
        {
            if (value <= 0) throw new ArgumentException("DPI must be greater than 0");
            _dpi = value;
        }
    }
}
