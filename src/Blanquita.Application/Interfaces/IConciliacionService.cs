using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IConciliacionService
{
    Task<ConciliacionResultDto> GetConciliacionAsync(int cashRegisterId, int shiftId, int cashierId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableBoxDto>> GetAvailableBoxesAsync(DateTime date, int? branchId = null, CancellationToken cancellationToken = default);
    Task SaveConciliacionCorteAsync(ConciliacionCorteDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConciliacionCorteDto>> GetConciliacionesByBranchAndDateAsync(string branchName, DateTime date, CancellationToken cancellationToken = default);
}

public record AvailableBoxDto
{
    public int Id { get; init; } // Internal Database ID
    public int ShiftId { get; init; } // CIDAPERTUR from FoxPro
    public string Name { get; init; } = string.Empty;
    public int IdContpaqi { get; init; } // CIDCAJA
    public int CashierId { get; init; } // CIDCAJER01
    public DateTime Time { get; init; } // CHORAAPER
    public DateTime? TimeClose { get; init; } // CHORACOR
    public int Status { get; init; } // CESTADOAPE
    public bool IsClosed => Status == 1;
}

public record ConciliacionResultDto
{
    public string BranchName { get; init; } = string.Empty;
    public string CashRegisterName { get; init; } = string.Empty;
    public string CashierName { get; init; } = string.Empty;
    public decimal CashCollected { get; init; } // From FoxPro Cvtaefect
    public decimal CardCollected { get; init; } // From FoxPro Cvtatarje
    public decimal ReturnsTotal { get; init; }  // From FoxPro Capimport2
    public decimal TotalSold => CashCollected + CardCollected;
    public decimal TotalRecolectado { get; init; } // From internal CashCollection
    public decimal EfectivoEsperado => TotalSold - Math.Abs(ReturnsTotal) - TotalRecolectado;
    public int Status { get; init; } // From FoxPro Cestadoape
    public bool IsClosed => Status == 1;
}
