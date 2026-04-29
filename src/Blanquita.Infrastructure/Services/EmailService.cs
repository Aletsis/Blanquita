using Blanquita.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguracionService _configService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguracionService configService, ILogger<EmailService> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, IEnumerable<string>? attachments = null)
    {
        try
        {
            var config = await _configService.ObtenerConfiguracionAsync();

            if (string.IsNullOrEmpty(config.SmtpServer))
            {
                _logger.LogWarning("El servidor SMTP no está configurado. No se puede enviar el correo.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.SmtpFromName ?? "Sistema Blanquita", config.SmtpFromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (File.Exists(attachment))
                    {
                        bodyBuilder.Attachments.Add(attachment);
                    }
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Ignorar validación de certificado SSL si es necesario (común en servidores locales/dev)
            // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(config.SmtpServer, config.SmtpPort, config.SmtpEnableSsl);
            
            if (!string.IsNullOrEmpty(config.SmtpUser))
            {
                await client.AuthenticateAsync(config.SmtpUser, config.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado exitosamente a {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {To}", to);
            throw;
        }
    }
}
