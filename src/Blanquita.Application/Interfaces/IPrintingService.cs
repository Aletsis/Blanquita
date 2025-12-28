using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IPrintingService
{
    Task PrintCashCutAsync(CashCutDto cashCut, string printerIp, int printerPort, CancellationToken cancellationToken = default);
    Task PrintCashCollectionAsync(CashCollectionDto cashCollection, string printerIp, int printerPort, CancellationToken cancellationToken = default);
    Task PrintTicketAsync(TicketDto ticket, CancellationToken cancellationToken = default);
    Task PrintZebraLabelAsync(ZebraLabelDto label, CancellationToken cancellationToken = default);
    Task<bool> TestPrinterConnectionAsync(string ipAddress, int port, CancellationToken cancellationToken = default);
}

