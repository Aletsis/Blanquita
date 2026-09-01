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

    Task<IEnumerable<ProductSalesReportDto>> GetProductSalesReportAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<string> productCodes,
        IEnumerable<string> series,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el reporte de pedidos abiertos directamente desde POS10008 donde CABIERTO = 1 y CCANCELADO = 0.
    /// </summary>
    Task<IEnumerable<OpenPedidoReportItemDto>> GetOpenPedidosReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        IEnumerable<string>? series = null,
        string? comanda = null,
        string? ruta = null,
        string? sucursalNombre = null,
        CancellationToken cancellationToken = default);
}
