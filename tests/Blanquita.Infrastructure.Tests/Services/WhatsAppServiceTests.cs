using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class WhatsAppServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguracionService> _configServiceMock;
    private readonly Mock<ILogger<WhatsAppService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public WhatsAppServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configServiceMock = new Mock<IConfiguracionService>();
        _loggerMock = new Mock<ILogger<WhatsAppService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnFalse_WhenUrlNotConfigured()
    {
        // Arrange
        var config = new ConfiguracionDto { WhatsAppServiceUrl = "" };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendMessageAsync("1234567890", "Hello");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSendPostRequest_WithCorrectHeadersAndBody()
    {
        // Arrange
        var config = new ConfiguracionDto 
        { 
            WhatsAppServiceUrl = "http://whatsapp-service:3001",
            WhatsAppApiKey = "my-secret-key"
        };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Set up mock HttpMessageHandler to intercept SendAsync
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendMessageAsync("5214441234567", "Testing message");

        // Assert
        Assert.True(result);

        // Verify request details
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://whatsapp-service:3001/send") &&
                req.Headers.Contains("X-API-Key") &&
                string.Join("", req.Headers.GetValues("X-API-Key")) == "my-secret-key"
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnFalse_WhenApiReturnsError()
    {
        // Arrange
        var config = new ConfiguracionDto 
        { 
            WhatsAppServiceUrl = "http://whatsapp-service:3001",
            WhatsAppApiKey = "my-secret-key"
        };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("{\"error\":\"Internal Server Error\"}")
            });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendMessageAsync("1234567890", "Fail test");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendDocumentAsync_ShouldReturnFalse_WhenUrlNotConfigured()
    {
        // Arrange
        var config = new ConfiguracionDto { WhatsAppServiceUrl = "" };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendDocumentAsync("1234567890", "base64data", "file.pdf", "application/pdf");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendDocumentAsync_ShouldSendPostRequest_WithCorrectHeadersAndBody()
    {
        // Arrange
        var config = new ConfiguracionDto 
        { 
            WhatsAppServiceUrl = "http://whatsapp-service:3001",
            WhatsAppApiKey = "my-secret-key"
        };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendDocumentAsync("1234567890", "base64data", "file.pdf", "application/pdf", "Invoice PDF");

        // Assert
        Assert.True(result);

        // Verify request details
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://whatsapp-service:3001/send-document") &&
                req.Headers.Contains("X-API-Key") &&
                string.Join("", req.Headers.GetValues("X-API-Key")) == "my-secret-key"
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendDocumentAsync_ShouldReturnFalse_WhenApiReturnsError()
    {
        // Arrange
        var config = new ConfiguracionDto 
        { 
            WhatsAppServiceUrl = "http://whatsapp-service:3001",
            WhatsAppApiKey = "my-secret-key"
        };
        _configServiceMock.Setup(s => s.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("{\"error\":\"Internal Server Error\"}")
            });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new WhatsAppService(
            _httpClientFactoryMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendDocumentAsync("1234567890", "base64data", "file.pdf", "application/pdf");

        // Assert
        Assert.False(result);
    }
}
