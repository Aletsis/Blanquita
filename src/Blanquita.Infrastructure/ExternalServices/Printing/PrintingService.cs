using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Constants;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace Blanquita.Infrastructure.ExternalServices.Printing;

public class PrintingService : IPrintingService
{
    private readonly ILogger<PrintingService> _logger;
    private readonly PrinterCommandBuilder _commandBuilder;
    private readonly ILabelDesignService _labelDesignService;

    public PrintingService(
        ILogger<PrintingService> logger,
        ILabelDesignService labelDesignService)
    {
        _logger = logger;
        _commandBuilder = new PrinterCommandBuilder();
        _labelDesignService = labelDesignService;
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

            // Obtener el diseño por defecto
            var design = await _labelDesignService.GetDefaultAsync(cancellationToken);
            if (design == null)
            {
                _logger.LogWarning("No default label design found, using fallback design");
                design = CreateFallbackDesign();
            }

            // Build ZPL commands for Zebra printer using the configured design
            var zplCommands = BuildZebraLabel(label, design);

            // Send to Zebra printer
            await SendToPrinterAsync(label.PrinterIp, label.PrinterPort, zplCommands, cancellationToken);

            _logger.LogInformation("Zebra label printed successfully for product {ProductCode} using design '{DesignName}'", 
                label.ProductCode, design.Name);
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

    private LabelDesignDto CreateFallbackDesign()
    {
        return new LabelDesignDto
        {
            Name = "Diseño Fallback",
            WidthInMm = 50m,
            HeightInMm = 30m,
            MarginTopInMm = 4m,
            MarginLeftInMm = 4m,
            ProductNameFontSize = 40,
            ProductCodeFontSize = 30,
            PriceFontSize = 50,
            BarcodeHeightInMm = 10m,
            BarcodeWidth = 2,
            Orientation = "N",
            IsDefault = true
        };
    }

    private byte[] BuildZebraLabel(ZebraLabelDto label, LabelDesignDto design)
    {
        var zplBuilder = new System.Text.StringBuilder();
        zplBuilder.AppendLine("^XA");
        
        // Soporte para caracteres UTF-8
        zplBuilder.AppendLine("^CI28"); 

        string o = string.IsNullOrEmpty(design.Orientation) ? "N" : design.Orientation;
        int dpi = label.PrinterDpi > 0 ? label.PrinterDpi : 203;

        if (design.Elements != null && design.Elements.Any())
        {
            // Dynamic generation based on elements
            foreach (var element in design.Elements)
            {
                int x = MmToDots(element.XMm, dpi);
                int y = MmToDots(element.YMm, dpi);
                string content = ResolveContent(element.Content, label);

                if (element.ElementType == "Text")
                {
                    // ^A0o,h,w (using font 0 scalable)
                    int scaledFontSize = ScaleFont(element.FontSize, dpi);
                    zplBuilder.AppendLine($"^FO{x},{y}^A0{o},{scaledFontSize},{scaledFontSize}^FD{content}^FS");
                }
                else if (element.ElementType == "Barcode")
                {
                    int h = element.HeightMm.HasValue ? MmToDots(element.HeightMm.Value, dpi) : ScaleFont(50, dpi); // Falback height if null
                    int w = element.BarWidth ?? design.BarcodeWidth;
                    
                    // ^BY width, ratio, height
                    zplBuilder.AppendLine($"^BY{w},3,{h}");
                    // ^BC orientation, height, flag, flag, flag, mode
                    zplBuilder.AppendLine($"^FO{x},{y}^BC{o},{h},Y,N,N^FD{content}^FS");
                }
            }
        }
        else
        {
            // Legacy / Fallback Mode (Hardcoded positions)
            
            // Calcular posiciones base en dots
            int curX = MmToDots(design.MarginLeftInMm, dpi);
            int curY = MmToDots(design.MarginTopInMm, dpi);
            
            // Espaciado entre elementos basado en milímetros (2mm)
            int gap = MmToDots(2, dpi); 

            // 1. Nombre del Producto
            int pNameSize = ScaleFont(design.ProductNameFontSize, dpi);
            zplBuilder.AppendLine($"^FO{curX},{curY}^A0{o},{pNameSize},{pNameSize}^FD{label.ProductName}^FS");
            
            // Actualizar Y
            curY += pNameSize + gap;

            // 2. Código (Texto)
            int pCodeSize = ScaleFont(design.ProductCodeFontSize, dpi);
            zplBuilder.AppendLine($"^FO{curX},{curY}^A0{o},{pCodeSize},{pCodeSize}^FDCodigo: {label.ProductCode}^FS");
            curY += pCodeSize + gap;

            // 3. Precio
            int priceSize = ScaleFont(design.PriceFontSize, dpi);
            zplBuilder.AppendLine($"^FO{curX},{curY}^A0{o},{priceSize},{priceSize}^FDPrecio: ${label.Price:F2}^FS");
            curY += priceSize + gap;

            // 4. Código de Barras
            int barcodeHeightDots = MmToDots(design.BarcodeHeightInMm, dpi);
            zplBuilder.AppendLine($"^BY{design.BarcodeWidth},3,{barcodeHeightDots}");
            zplBuilder.AppendLine($"^FO{curX},{curY}^BC{o},{barcodeHeightDots},Y,N,N^FD{label.ProductCode}^FS");
        }

        // Cantidad
        zplBuilder.AppendLine($"^PQ{label.Quantity}");
        zplBuilder.AppendLine("^XZ");

        return System.Text.Encoding.UTF8.GetBytes(zplBuilder.ToString());
    }

    private static int MmToDots(decimal mm, int dpi)
    {
        return (int)Math.Round(mm / 25.4m * dpi);
    }

    private int ScaleFont(int fontSize, int printerDpi)
    {
        // Assuming fontSize is based on StandardDPI (300)
        return (int)Math.Round(fontSize * (printerDpi / 300.0));
    }

    private string ResolveContent(string template, ZebraLabelDto label)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        
        // Replace known placeholders
        // We can extend this with a robust template engine if needed later
        return template
            .Replace(LabelVariables.ProductName, label.ProductName)
            .Replace(LabelVariables.ProductCode, label.ProductCode)
            .Replace(LabelVariables.Price, $"${label.Price:F2}") // Default formatting
            .Replace(LabelVariables.PriceNoFormat, label.Price.ToString())
            .Replace(LabelVariables.Date, DateTime.Now.ToString("dd/MM/yyyy")); 
    }
}
