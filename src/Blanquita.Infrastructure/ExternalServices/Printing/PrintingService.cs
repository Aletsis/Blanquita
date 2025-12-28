using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.Printing;

public class PrintingService : IPrintingService
{
    private readonly ILogger<PrintingService> _logger;
    private readonly PrinterCommandBuilder _commandBuilder;

    public PrintingService(ILogger<PrintingService> logger)
    {
        _logger = logger;
        _commandBuilder = new PrinterCommandBuilder();
    }

    public async Task PrintCashCutAsync(CashCutDto cashCut, string printerIp, int printerPort, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Printing cash cut for register {CashRegister}, Total: {Total} to {Ip}:{Port}",
                cashCut.CashRegisterName,
                cashCut.GrandTotal,
                printerIp,
                printerPort);

            // Build print commands
            var commands = _commandBuilder.BuildCashCutTicket(cashCut);

            // Send to printer
            await SendToPrinterAsync(printerIp, printerPort, commands.ToArray(), cancellationToken);
            
            _logger.LogDebug("Cash cut ticket printed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing cash cut for register {CashRegister}", cashCut.CashRegisterName);
            throw;
        }
    }

    public async Task PrintCashCollectionAsync(CashCollectionDto cashCollection, string printerIp, int printerPort, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Printing cash collection. Folio: {Folio}, Register: {CashRegister}, Amount: {Amount} to {Ip}:{Port}",
                cashCollection.Folio,
                cashCollection.CashRegisterName,
                cashCollection.TotalAmount,
                printerIp,
                printerPort);

            // Build print commands
            var commands = _commandBuilder.BuildCashCollectionTicket(cashCollection);

            // Send to printer
            await SendToPrinterAsync(printerIp, printerPort, commands.ToArray(), cancellationToken);

            _logger.LogDebug("Cash collection ticket printed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing cash collection. Folio: {Folio}", cashCollection.Folio);
            throw;
        }
    }

    public async Task PrintTicketAsync(TicketDto ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Printing custom ticket: {Title}", ticket.Title);

            // Build print commands
            var commands = _commandBuilder.BuildCustomTicket(ticket);

            // Send to printer
            await SendToPrinterAsync(ticket.PrinterIp, ticket.PrinterPort, commands.ToArray(), cancellationToken);

            _logger.LogInformation("Ticket printed successfully to {PrinterIp}:{Port}", ticket.PrinterIp, ticket.PrinterPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing ticket to {PrinterIp}:{Port}", ticket.PrinterIp, ticket.PrinterPort);
            throw;
        }
    }

    public async Task PrintZebraLabelAsync(ZebraLabelDto label, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Printing Zebra label for product {ProductCode}", label.ProductCode);

            // Build ZPL commands for Zebra printer
            var zplCommands = BuildZebraLabel(label);

            // Send to Zebra printer
            await SendToPrinterAsync(label.PrinterIp, label.PrinterPort, zplCommands, cancellationToken);

            _logger.LogInformation("Zebra label printed successfully for product {ProductCode}", label.ProductCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing Zebra label for product {ProductCode}", label.ProductCode);
            throw;
        }
    }

    public async Task<bool> TestPrinterConnectionAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Testing printer connection to {IpAddress}:{Port}", ipAddress, port);

            using var printerService = new PrinterNetworkService(ipAddress, port);
            await printerService.ConnectAsync(cancellationToken);
            printerService.Disconnect();

            _logger.LogInformation("Printer connection test successful for {IpAddress}:{Port}", ipAddress, port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Printer connection test failed for {IpAddress}:{Port}", ipAddress, port);
            return false;
        }
    }

    private async Task SendToPrinterAsync(string ipAddress, int port, byte[] data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending {DataSize} bytes to printer {IpAddress}:{Port}", data.Length, ipAddress, port);

        using var printerService = new PrinterNetworkService(ipAddress, port);

        try
        {
            await printerService.ConnectAsync(cancellationToken);
            await printerService.SendRawDataAsync(data, cancellationToken);
            _logger.LogDebug("Data sent successfully to printer");
        }
        finally
        {
            printerService.Disconnect();
        }
    }

    private byte[] BuildZebraLabel(ZebraLabelDto label)
    {
        // ZPL (Zebra Programming Language) commands
        var zpl = $@"
^XA
^FO50,50^A0N,50,50^FD{label.ProductName}^FS
^FO50,120^A0N,40,40^FDCodigo: {label.ProductCode}^FS
^FO50,180^A0N,60,60^FDPrecio: ${label.Price:F2}^FS
^FO50,260^BY3^BCN,100,Y,N,N^FD{label.ProductCode}^FS
^PQ{label.Quantity}
^XZ
";
        return System.Text.Encoding.UTF8.GetBytes(zpl);
    }
}
