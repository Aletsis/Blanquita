namespace Blanquita.Domain.Repositories;

/// <summary>
/// Filtros de búsqueda de recolecciones; se aplican en la base de datos.
/// Las fechas deben venir ya convertidas a UTC.
/// </summary>
public sealed record CashCollectionSearchCriteria
{
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public string? CashRegisterName { get; init; }
    public bool? IsForCashCut { get; init; }
    public int Skip { get; init; }
    public int? Take { get; init; }
}

/// <summary>
/// Filtros, orden y paginación de búsqueda de cortes; se aplican en la base de datos.
/// Las fechas deben venir ya convertidas a UTC.
/// </summary>
public sealed record CashCutSearchCriteria
{
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public string? BranchName { get; init; }
    public string? CashRegisterName { get; init; }
    public string? CashierName { get; init; }
    public string? SupervisorName { get; init; }
    public decimal? MinGrandTotal { get; init; }
    public decimal? MaxGrandTotal { get; init; }
    public string? SortColumn { get; init; }
    public bool SortAscending { get; init; } = true;
    public int Skip { get; init; }
    public int? Take { get; init; }
}
