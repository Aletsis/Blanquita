using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Services;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class InvoiceJobServiceTests
{
    private readonly Mock<IClientCatalogRepository> _clientRepositoryMock = new();
    private readonly Mock<IFoxProDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IConfiguracionService> _configServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ISentInvoiceLogRepository> _logRepositoryMock = new();
    private readonly Mock<IFileSystemService> _fileSystemServiceMock = new();
    private readonly InvoiceJobService _service;

    public InvoiceJobServiceTests()
    {
        _configServiceMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { FacturasPath = "facturas", SmtpFromName = "Blanquita" });

        // Por defecto todas las carpetas y archivos existen y nada se ha enviado
        _fileSystemServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        _logRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _service = new InvoiceJobService(
            _clientRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _configServiceMock.Object,
            _emailServiceMock.Object,
            _logRepositoryMock.Object,
            _fileSystemServiceMock.Object,
            new Mock<ILogger<InvoiceJobService>>().Object);
    }

    private void SetupData(IEnumerable<ClientSearchDto> clients, IEnumerable<InvoiceDto> invoices)
    {
        _clientRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(clients);
        _documentRepositoryMock.Setup(x => x.GetRecentInvoicesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);
    }

    [Fact]
    public async Task ProcessAndSendInvoicesAsync_ShouldContinueWithOtherInvoices_WhenOneFails()
    {
        SetupData(
            new[] { new ClientSearchDto { Id = 1, Code = "C1", Name = "Cliente", Email = "c1@test.com" } },
            new[]
            {
                new InvoiceDto { ClientId = 1, Serie = "A", Folio = 1, FileName = "F1" },
                new InvoiceDto { ClientId = 1, Serie = "A", Folio = 2, FileName = "F2" }
            });

        _emailServiceMock.Setup(x => x.SendEmailAsync("c1@test.com", It.Is<string>(s => s.Contains("A1")), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>()))
            .ThrowsAsync(new InvalidOperationException("SMTP caído"));

        await _service.ProcessAndSendInvoicesAsync();

        // La segunda factura se envía y registra aunque la primera haya fallado
        _logRepositoryMock.Verify(x => x.AddAsync(It.Is<SentInvoiceLog>(l => l.FileName == "F2"), It.IsAny<CancellationToken>()), Times.Once);
        _logRepositoryMock.Verify(x => x.AddAsync(It.Is<SentInvoiceLog>(l => l.FileName == "F1"), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAndSendInvoicesAsync_ShouldNotAbort_WhenCatalogHasDuplicateClientIds()
    {
        SetupData(
            new[]
            {
                new ClientSearchDto { Id = 1, Code = "C1", Name = "Cliente", Email = "c1@test.com" },
                new ClientSearchDto { Id = 1, Code = "C1-DUP", Name = "Duplicado", Email = "dup@test.com" }
            },
            new[] { new InvoiceDto { ClientId = 1, Serie = "A", Folio = 1, FileName = "F1" } });

        await _service.ProcessAndSendInvoicesAsync();

        _emailServiceMock.Verify(x => x.SendEmailAsync("c1@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAndSendInvoicesAsync_ShouldSkipInvoicesAlreadySent()
    {
        SetupData(
            new[] { new ClientSearchDto { Id = 1, Code = "C1", Name = "Cliente", Email = "c1@test.com" } },
            new[] { new InvoiceDto { ClientId = 1, Serie = "A", Folio = 1, FileName = "F1" } });

        _logRepositoryMock.Setup(x => x.ExistsAsync("C1", "F1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _service.ProcessAndSendInvoicesAsync();

        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAndSendInvoicesAsync_ShouldEncodeClientNameInEmailBody()
    {
        SetupData(
            new[] { new ClientSearchDto { Id = 1, Code = "C1", Name = "<script>x</script>", Email = "c1@test.com" } },
            new[] { new InvoiceDto { ClientId = 1, Serie = "A", Folio = 1, FileName = "F1" } });

        string? body = null;
        _emailServiceMock.Setup(x => x.SendEmailAsync("c1@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>()))
            .Callback<string, string, string, IEnumerable<string>?>((_, _, b, _) => body = b)
            .Returns(Task.CompletedTask);

        await _service.ProcessAndSendInvoicesAsync();

        Assert.NotNull(body);
        Assert.DoesNotContain("<script>", body);
        Assert.Contains("&lt;script&gt;", body);
    }
}
