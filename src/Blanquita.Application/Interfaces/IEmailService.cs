using System.Threading.Tasks;

namespace Blanquita.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, IEnumerable<string>? attachments = null);
}
