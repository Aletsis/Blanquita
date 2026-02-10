using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Entities;

public class Return : BaseEntity
{
    public string IdDocument { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public decimal Net { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    
    public List<ReturnDetail> Details { get; set; } = new();
}
