using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Entities;

public class ReturnDetail : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public double Units { get; set; }
    public decimal Net { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    
    // Foreign Key? Or just reference via IdDocument?
    // In Clean Architecture with Repository pattern over DBF, we usually manage relations manually in repo.
}
