using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Blanquita.Application.DTOs;

namespace Blanquita.Infrastructure.Services;

public class InvoiceJobService : IInvoiceJobService
{
    private readonly IClientCatalogRepository _clientRepository;
    private readonly IFoxProDocumentRepository _documentRepository;
    private readonly IConfiguracionService _configService;
    private readonly IEmailService _emailService;
    private readonly BlanquitaDbContext _dbContext;
    private readonly ILogger<InvoiceJobService> _logger;

    public InvoiceJobService(
        IClientCatalogRepository clientRepository,
        IFoxProDocumentRepository documentRepository,
        IConfiguracionService configService,
        IEmailService emailService,
        BlanquitaDbContext dbContext,
        ILogger<InvoiceJobService> logger)
    {
        _clientRepository = clientRepository;
        _documentRepository = documentRepository;
        _configService = configService;
        _emailService = emailService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ProcessAndSendInvoicesAsync()
    {
        _logger.LogInformation("Iniciando trabajo de envío automático de facturas");

        var clientsWithoutEmail = new List<string>();
        var invoicesMissingFiles = new List<(string ClientCode, string InvoiceInfo)>();
        int totalSent = 0;

        try
        {
            var config = await _configService.ObtenerConfiguracionAsync();
            if (string.IsNullOrEmpty(config.FacturasPath))
            {
                _logger.LogWarning("Ruta de facturas no configurada. Abortando trabajo.");
                return;
            }

            // 1. Obtener todos los clientes (de FoxPro)
            var allClients = await _clientRepository.GetAllAsync();
            var clientsToProcess = allClients.ToList();

            _logger.LogInformation("Se encontraron {Count} clientes totales para procesar", clientsToProcess.Count);

            foreach (var client in clientsToProcess)
            {
                // Caso A: Cliente sin correo
                if (string.IsNullOrEmpty(client.Email))
                {
                    clientsWithoutEmail.Add($"{client.Code} - {client.Name}");
                    continue;
                }

                try
                {
                    // 2. Obtener facturas del cliente desde FoxPro
                    var invoices = await _documentRepository.GetInvoicesByClientIdAsync(client.Id);
                    
                    // Solo procesar facturas recientes (últimos 7 días)
                    var recentInvoices = invoices.Where(i => i.Fecha >= DateTime.Today.AddDays(-7)).ToList();

                    if (!recentInvoices.Any()) continue;

                    var clientFolder = Path.Combine(config.FacturasPath, client.Code);
                    
                    foreach (var invoice in recentInvoices)
                    {
                        // 3. Verificar si ya fue enviada
                        var alreadySent = await _dbContext.SentInvoiceLogs
                            .AnyAsync(l => l.ClientCode == client.Code && l.FileName == invoice.FileName);

                        if (alreadySent) continue;

                        // 4. Verificar existencia de archivos físicos
                        if (!Directory.Exists(clientFolder))
                        {
                            invoicesMissingFiles.Add((client.Code, $"Carpeta no encontrada: {client.Code} (Factura {invoice.Serie}{invoice.Folio})"));
                            continue;
                        }

                        var xmlPath = Path.Combine(clientFolder, invoice.FileName + ".xml");
                        var pdfPath = Path.Combine(clientFolder, invoice.FileName + ".pdf");
                        var attachments = new List<string>();

                        if (File.Exists(xmlPath)) attachments.Add(xmlPath);
                        if (File.Exists(pdfPath)) attachments.Add(pdfPath);

                        // Caso B: Factura sin archivos físicos
                        if (!attachments.Any())
                        {
                            invoicesMissingFiles.Add((client.Code, $"Archivos no encontrados: {invoice.FileName} (Factura {invoice.Serie}{invoice.Folio})"));
                            continue;
                        }

                        // 5. Enviar correo al cliente
                        var subject = $"Factura {invoice.Serie}{invoice.Folio} - {config.SmtpFromName}";
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; color: #333;'>
                                <h2 style='color: #1e88e5;'>Hola {client.Name},</h2>
                                <p>Le adjuntamos su factura correspondiente a su compra del día <strong>{invoice.Fecha:dd/MM/yyyy}</strong>.</p>
                                <p><strong>Detalles del Documento:</strong></p>
                                <ul>
                                    <li>Serie: {invoice.Serie}</li>
                                    <li>Folio: {invoice.Folio}</li>
                                </ul>
                                <p>Agradecemos su preferencia.</p>
                                <hr style='border: 0; border-top: 1px solid #eee;' />
                                <p style='font-size: 0.8rem; color: #999;'>Este es un envío automático del Sistema Blanquita. Por favor no responda a este mensaje.</p>
                            </div>";

                        await _emailService.SendEmailAsync(client.Email, subject, body, attachments);

                        // 6. Registrar envío en la BD
                        _dbContext.SentInvoiceLogs.Add(new SentInvoiceLog
                        {
                            ClientCode = client.Code,
                            FileName = invoice.FileName,
                            SentAt = DateTime.UtcNow
                        });

                        await _dbContext.SaveChangesAsync();
                        totalSent++;
                        
                        _logger.LogInformation("Factura {FileName} enviada exitosamente al cliente {Code}", invoice.FileName, client.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar facturas para el cliente {Code}", client.Code);
                }
            }

            // 7. Enviar correo de resumen a las encargadas
            await SendSummaryEmailAsync(config, totalSent, clientsWithoutEmail, invoicesMissingFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fatal en el trabajo de envío de facturas");
        }
    }

    private async Task SendSummaryEmailAsync(ConfiguracionDto config, int totalSent, List<string> clientsWithoutEmail, List<(string ClientCode, string InvoiceInfo)> invoicesMissingFiles)
    {
        if (string.IsNullOrEmpty(config.AlertEmails)) return;

        try
        {
            var alertEmails = config.AlertEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!alertEmails.Any()) return;

            var subject = $"Resumen de Envío Automático de Facturas - {DateTime.Today:dd/MM/yyyy}";
            
            var bodyBuilder = new System.Text.StringBuilder();
            bodyBuilder.Append("<div style='font-family: Arial, sans-serif; color: #333;'>");
            bodyBuilder.Append("<h2 style='color: #2e7d32;'>Resumen de Ejecución</h2>");
            bodyBuilder.Append($"<p>El proceso automático de facturación ha finalizado.</p>");
            bodyBuilder.Append($"<p><strong>Facturas enviadas con éxito:</strong> <span style='color: #2e7d32; font-weight: bold;'>{totalSent}</span></p>");

            if (clientsWithoutEmail.Any())
            {
                bodyBuilder.Append("<h3 style='color: #f57c00;'>Clientes sin Correo Configurado</h3>");
                bodyBuilder.Append("<p>Los siguientes clientes tienen facturas recientes pero no tienen un correo electrónico en su ficha:</p>");
                bodyBuilder.Append("<ul>");
                foreach (var client in clientsWithoutEmail.Take(50)) // Limitar para no hacer un correo gigante
                {
                    bodyBuilder.Append($"<li>{client}</li>");
                }
                if (clientsWithoutEmail.Count > 50) bodyBuilder.Append($"<li>... y {clientsWithoutEmail.Count - 50} más.</li>");
                bodyBuilder.Append("</ul>");
            }

            if (invoicesMissingFiles.Any())
            {
                bodyBuilder.Append("<h3 style='color: #d32f2f;'>Facturas con Archivos Faltantes</h3>");
                bodyBuilder.Append("<p>Se encontraron registros de facturas en el sistema, pero los archivos PDF/XML no están en el servidor:</p>");
                bodyBuilder.Append("<ul>");
                foreach (var item in invoicesMissingFiles.Take(50))
                {
                    bodyBuilder.Append($"<li><strong>Cliente {item.ClientCode}:</strong> {item.InvoiceInfo}</li>");
                }
                if (invoicesMissingFiles.Count > 50) bodyBuilder.Append($"<li>... y {invoicesMissingFiles.Count - 50} más.</li>");
                bodyBuilder.Append("</ul>");
            }

            bodyBuilder.Append("<hr/><p style='font-size: 0.8rem; color: #999;'>Este es un reporte técnico generado automáticamente por Hangfire.</p>");
            bodyBuilder.Append("</div>");

            foreach (var email in alertEmails)
            {
                await _emailService.SendEmailAsync(email, subject, bodyBuilder.ToString());
            }
            
            _logger.LogInformation("Resumen de facturación enviado a {Count} destinatarios", alertEmails.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar el correo de resumen de facturación");
        }
    }
}
