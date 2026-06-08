using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

public interface IFoxProPedidoRepository
{
    Task<IEnumerable<PedidoDto>> SearchPedidosAsync(
        DateTime date, 
        IEnumerable<string> series, 
        string? comanda = null, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PedidoItemDto>> GetPedidoItemsAsync(string idDocumento, CancellationToken cancellationToken = default);

    Task<IEnumerable<PedidoDto>> SearchRutasAsync(
        DateTime date, 
        IEnumerable<string> series, 
        string? ruta = null, 
        string? repartidorNomina = null, 
        CancellationToken cancellationToken = default);

    Task<PedidoDto?> GetBySeriesAndFolioAsync(string series, string folio, CancellationToken cancellationToken = default);

    Task<IEnumerable<PedidoDto>> GetSalesReportByFiltersAsync(
        DateTime startDate,
        DateTime endDate,
        TimeSpan startTime,
        TimeSpan endTime,
        IEnumerable<string> series,
        CancellationToken cancellationToken = default);
}
