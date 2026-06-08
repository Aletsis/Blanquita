using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

public interface IFoxProShiftRepository
{
    Task<ShiftConciliationDataDto?> GetShiftDataAsync(int internalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftConciliationDataDto>> GetTodayShiftsAsync(DateTime date, CancellationToken cancellationToken = default);
}

public record ShiftConciliationDataDto
{
    public int InternalId { get; init; } // CIDPOS01 from FoxPro
    public int IdContpaqi { get; init; } // CIDCAJA
    public int CashierId { get; init; } // CIDCAJER01
    public DateTime OpeningTime { get; init; } // CHORAAPER
    public DateTime? ClosingTime { get; init; } // CHORACOR
    public int Status { get; init; } // Cestadoape
    public decimal CashCollected { get; init; } // Cvtaefect
    public decimal CardCollected { get; init; } // Cvtatarje
    public decimal ReturnsTotal { get; init; } // Capimport2
    public decimal TotalSold => CashCollected + CardCollected;
}
