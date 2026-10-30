using System.Net;
using System.Text;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class CommercialApiServiceTests
{
    /// <summary>
    /// Simula la API Comercial: responde al login con tokens consecutivos y delega las demás peticiones.
    /// </summary>
    private sealed class FakeCommercialApiHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _onApiRequest;
        private int _loginCount;

        public FakeCommercialApiHandler(Func<HttpRequestMessage, HttpResponseMessage> onApiRequest)
        {
            _onApiRequest = onApiRequest;
        }

        public int LoginCount => _loginCount;
        public List<string> RequestUris { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!.ToString());

            if (request.RequestUri!.AbsolutePath.EndsWith("/api/auth/login"))
            {
                var token = $"token-{Interlocked.Increment(ref _loginCount)}";
                return Task.FromResult(Json($"{{\"token\":\"{token}\"}}"));
            }

            return Task.FromResult(_onApiRequest(request));
        }
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static CommercialApiService CreateService(FakeCommercialApiHandler handler, string apiUrl)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler, disposeHandler: false));

        var config = new Mock<IConfiguracionService>();
        config.Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto { CommercialApiUrl = apiUrl, CommercialApiKey = "api-key" });

        return new CommercialApiService(factory.Object, config.Object, new Mock<ILogger<CommercialApiService>>().Object);
    }

    // El caché de tokens es estático: cada prueba usa un usuario y URL propios
    private static string UniqueUser() => $"user-{Guid.NewGuid():N}";

    [Fact]
    public async Task ShouldRenewTokenAndRetry_WhenApiRespondsUnauthorized()
    {
        var handler = new FakeCommercialApiHandler(request =>
            request.Headers.Authorization?.Parameter == "token-1"
                ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                : Json("[]"));
        var service = CreateService(handler, $"http://api-{Guid.NewGuid():N}");

        var result = await service.GetComprasAsync(null, null, null, null, null, UniqueUser());

        Assert.Empty(result);
        Assert.Equal(2, handler.LoginCount); // login inicial + renovación tras el 401
    }

    [Fact]
    public async Task ShouldThrow_WhenApiStillRespondsUnauthorizedAfterRetry()
    {
        var handler = new FakeCommercialApiHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var service = CreateService(handler, $"http://api-{Guid.NewGuid():N}");

        await Assert.ThrowsAnyAsync<Exception>(() => service.GetComprasAsync(null, null, null, null, null, UniqueUser()));

        // Solo un reintento: no debe quedar en un ciclo de logins
        Assert.Equal(2, handler.LoginCount);
    }

    [Fact]
    public async Task ShouldReuseCachedToken_ForSameUserAndApi()
    {
        var handler = new FakeCommercialApiHandler(_ => Json("[]"));
        var service = CreateService(handler, $"http://api-{Guid.NewGuid():N}");
        var user = UniqueUser();

        await service.GetComprasAsync(null, null, null, null, null, user);
        await service.GetComprasAsync(null, null, null, null, null, user);

        Assert.Equal(1, handler.LoginCount);
    }

    [Fact]
    public async Task ShouldNotReuseToken_WhenApiUrlChanges()
    {
        var handler = new FakeCommercialApiHandler(_ => Json("[]"));
        var user = UniqueUser();

        await CreateService(handler, $"http://api-a-{Guid.NewGuid():N}").GetComprasAsync(null, null, null, null, null, user);
        await CreateService(handler, $"http://api-b-{Guid.NewGuid():N}").GetComprasAsync(null, null, null, null, null, user);

        Assert.Equal(2, handler.LoginCount);
    }

    [Fact]
    public async Task ShouldLoginOnce_WhenConcurrentRequestsNeedToken()
    {
        var handler = new FakeCommercialApiHandler(_ => Json("[]"));
        var service = CreateService(handler, $"http://api-{Guid.NewGuid():N}");
        var user = UniqueUser();

        await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => service.GetComprasAsync(null, null, null, null, null, user)));

        Assert.Equal(1, handler.LoginCount);
    }

    [Fact]
    public async Task ShouldFormatFolioWithInvariantCulture()
    {
        var handler = new FakeCommercialApiHandler(_ => Json("[]"));
        var service = CreateService(handler, $"http://api-{Guid.NewGuid():N}");

        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE"); // usa coma decimal
        try
        {
            await service.GetComprasAsync(null, 12.5, null, null, null, UniqueUser());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }

        Assert.Contains(handler.RequestUris, u => u.Contains("folio=12.5"));
    }
}
