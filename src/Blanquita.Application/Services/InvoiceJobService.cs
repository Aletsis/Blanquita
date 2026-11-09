using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Blanquita.Application.DTOs;

namespace Blanquita.Application.Services;

public class InvoiceJobService : IInvoiceJobService
{
    private readonly IClientCatalogRepository _clientRepository;
    private readonly IFoxProDocumentRepository _documentRepository;
    private readonly IConfiguracionService _configService;
    private readonly IEmailService _emailService;
    private readonly ISentInvoiceLogRepository _logRepository;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<InvoiceJobService> _logger;

    public InvoiceJobService(
        IClientCatalogRepository clientRepository,
        IFoxProDocumentRepository documentRepository,
        IConfiguracionService configService,
        IEmailService emailService,
        ISentInvoiceLogRepository logRepository,
        IFileSystemService fileSystemService,
        ILogger<InvoiceJobService> logger)
    {
        _clientRepository = clientRepository;
        _documentRepository = documentRepository;
        _configService = configService;
        _emailService = emailService;
        _logRepository = logRepository;
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    public async Task ProcessAndSendInvoicesAsync()
    {
        _logger.LogInformation("Iniciando trabajo de envío automático de facturas");

        var clientsWithoutEmail = new List<string>();
        var invoicesMissingFiles = new List<(string ClientCode, string InvoiceInfo)>();
        var failedInvoices = new List<(string ClientCode, string InvoiceInfo)>();
        int totalSent = 0;

        try
        {
            var config = await _configService.ObtenerConfiguracionAsync();
            if (string.IsNullOrEmpty(config.FacturasPath))
            {
                _logger.LogWarning("Ruta de facturas no configurada. Abortando trabajo.");
                return;
            }

            // 1. Obtener todas las facturas recientes de los últimos 7 días en una sola pasada reversa
            var limitDate = DateTime.Today.AddDays(-7);
            var recentInvoices = await _documentRepository.GetRecentInvoicesAsync(limitDate);
            var recentInvoicesByClient = recentInvoices
                .GroupBy(i => i.ClientId)
                .ToDictionary(g => g.Key, g => g.ToList());

            _logger.LogInformation("Se encontraron facturas recientes para {Count} clientes", recentInvoicesByClient.Count);

            // 2. Obtener todos los clientes para cruzar información.
            // Si el catálogo DBF trae Ids repetidos se usa el primero, en lugar de abortar todo el trabajo.
            var allClients = await _clientRepository.GetAllAsync();
            var clientMap = allClients
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.First());

            // 3. Procesar clientes con facturas recientes
            foreach (var group in recentInvoicesByClient)
            {
                var clientId = group.Key;
                var clientInvoices = group.Value;

                if (!clientMap.TryGetValue(clientId, out var client))
                {
                    _logger.LogWarning("Se encontró factura para el ClientId {ClientId} pero no existe en el catálogo de clientes", clientId);
                    continue;
                }

                // Caso A: Cliente sin correo
                if (string.IsNullOrEmpty(client.Email))
                {
                    clientsWithoutEmail.Add($"{client.Code} - {client.Name}");
                    continue;
                }

                var clientFolder = Path.Combine(config.FacturasPath, client.Code);

                foreach (var invoice in clientInvoices)
                {
                    // Un error en una factura no debe impedir el envío de las demás
                    try
                    {
                        if (await TrySendInvoiceAsync(config, client, invoice, clientFolder, invoicesMissingFiles))
                        {
                            totalSent++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedInvoices.Add((client.Code, $"{invoice.Serie}{invoice.Folio}: {ex.Message}"));
                        _logger.LogError(ex, "Error al enviar la factura {FileName} al cliente {Code}", invoice.FileName, client.Code);
                    }
                }
            }

            // 7. Enviar correo de resumen a las encargadas
            await SendSummaryEmailAsync(config, totalSent, clientsWithoutEmail, invoicesMissingFiles, failedInvoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fatal en el trabajo de envío de facturas");
        }
    }

    /// <summary>
    /// Envía una factura si aún no se ha enviado y tiene archivos. Devuelve true si se envió.
    /// </summary>
    private async Task<bool> TrySendInvoiceAsync(
        ConfiguracionDto config,
        ClientSearchDto client,
        InvoiceDto invoice,
        string clientFolder,
        List<(string ClientCode, string InvoiceInfo)> invoicesMissingFiles)
    {
        // 4. Verificar si ya fue enviada
        if (await _logRepository.ExistsAsync(client.Code, invoice.FileName))
        {
            return false;
        }

        // 5. Verificar existencia de archivos físicos
        if (!_fileSystemService.DirectoryExists(clientFolder))
        {
            invoicesMissingFiles.Add((client.Code, $"Carpeta no encontrada: {client.Code} (Factura {invoice.Serie}{invoice.Folio})"));
            return false;
        }

        var xmlPath = Path.Combine(clientFolder, invoice.FileName + ".xml");
        var pdfPath = Path.Combine(clientFolder, invoice.FileName + ".pdf");
        var attachments = new List<string>();

        if (_fileSystemService.FileExists(xmlPath)) attachments.Add(xmlPath);
        if (_fileSystemService.FileExists(pdfPath)) attachments.Add(pdfPath);

        // Caso B: Factura sin archivos físicos
        if (!attachments.Any())
        {
            invoicesMissingFiles.Add((client.Code, $"Archivos no encontrados: {invoice.FileName} (Factura {invoice.Serie}{invoice.Folio})"));
            return false;
        }

        // 6. Enviar correo al cliente (los datos del DBF se codifican para no inyectar HTML)
        var subject = $"Factura {invoice.Serie}{invoice.Folio} - {config.SmtpFromName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #1e88e5;'>Hola {Html(client.Name)},</h2>
                <p>Le adjuntamos su factura correspondiente a su compra del día <strong>{invoice.Fecha:dd/MM/yyyy}</strong>.</p>
                <p><strong>Detalles del Documento:</strong></p>
                <ul>
                    <li>Serie: {Html(invoice.Serie)}</li>
                    <li>Folio: {invoice.Folio}</li>
                </ul>
                <p>Agradecemos su preferencia.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='font-size: 0.8rem; color: #999;'>Este es un envío automático del Sistema Blanquita. Por favor no responda a este mensaje.</p>
            </div>";

        await _emailService.SendEmailAsync(client.Email, subject, body, attachments);

        // 7. Registrar envío en la BD. Si falla, el correo ya salió: se registra el error para no ocultarlo,
        // pero se cuenta como enviada (el índice único evita duplicar el registro si otro proceso lo insertó).
        try
        {
            await _logRepository.AddAsync(new SentInvoiceLog
            {
                ClientCode = client.Code,
                FileName = invoice.FileName,
                SentAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "La factura {FileName} se envió al cliente {Code} pero no se pudo registrar el envío; podría reenviarse en la siguiente ejecución",
                invoice.FileName, client.Code);
        }

        _logger.LogInformation("Factura {FileName} enviada exitosamente al cliente {Code}", invoice.FileName, client.Code);
        return true;
    }

    private static string Html(string? value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

    private async Task SendSummaryEmailAsync(ConfiguracionDto config, int totalSent, List<string> clientsWithoutEmail, List<(string ClientCode, string InvoiceInfo)> invoicesMissingFiles, List<(string ClientCode, string InvoiceInfo)> failedInvoices)
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
                    bodyBuilder.Append($"<li>{Html(client)}</li>");
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
                    bodyBuilder.Append($"<li><strong>Cliente {Html(item.ClientCode)}:</strong> {Html(item.InvoiceInfo)}</li>");
                }
                if (invoicesMissingFiles.Count > 50) bodyBuilder.Append($"<li>... y {invoicesMissingFiles.Count - 50} más.</li>");
                bodyBuilder.Append("</ul>");
            }

            if (failedInvoices.Any())
            {
                bodyBuilder.Append("<h3 style='color: #d32f2f;'>Facturas con Error de Envío</h3>");
                bodyBuilder.Append("<p>No se pudieron enviar las siguientes facturas; se reintentarán en la próxima ejecución:</p>");
                bodyBuilder.Append("<ul>");
                foreach (var item in failedInvoices.Take(50))
                {
                    bodyBuilder.Append($"<li><strong>Cliente {Html(item.ClientCode)}:</strong> {Html(item.InvoiceInfo)}</li>");
                }
                if (failedInvoices.Count > 50) bodyBuilder.Append($"<li>... y {failedInvoices.Count - 50} más.</li>");
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
