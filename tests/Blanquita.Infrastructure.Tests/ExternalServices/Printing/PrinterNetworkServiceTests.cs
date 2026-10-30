using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Blanquita.Infrastructure.ExternalServices.Printing;
using Xunit;

namespace Blanquita.Infrastructure.Tests.ExternalServices.Printing;

public class PrinterNetworkServiceTests
{
    private static int GetUnusedPort()
    {
        // Abrir y cerrar un listener deja un puerto libre en el que nadie escucha (conexión rechazada)
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task SendRawDataAsync_ShouldDeliverBytes_WhenPrinterIsListening()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        try
        {
            var receiveTask = Task.Run(async () =>
            {
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            });

            var data = new byte[] { 0x1B, 0x40, 0x41, 0x42 };
            using (var printer = new PrinterNetworkService("127.0.0.1", port))
            {
                await printer.ConnectAsync();
                await printer.SendRawDataAsync(data);
            }

            var received = await receiveTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(data, received);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task ConnectAsync_ShouldThrowTimeout_AfterTotalTimeout_WhenConnectionIsRefused()
    {
        var port = GetUnusedPort();
        using var printer = new PrinterNetworkService("127.0.0.1", port,
            attemptTimeout: TimeSpan.FromMilliseconds(500),
            totalTimeout: TimeSpan.FromMilliseconds(1500),
            retryDelay: TimeSpan.FromMilliseconds(300));

        var sw = Stopwatch.StartNew();
        await Assert.ThrowsAsync<TimeoutException>(() => printer.ConnectAsync());
        sw.Stop();

        // Debe agotar el tiempo total (con reintentos pausados), no fallar al primer rechazo ni colgarse
        Assert.InRange(sw.Elapsed, TimeSpan.FromMilliseconds(1200), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConnectAsync_ShouldThrowOperationCanceled_WhenCallerCancels()
    {
        var port = GetUnusedPort();
        using var printer = new PrinterNetworkService("127.0.0.1", port,
            totalTimeout: TimeSpan.FromSeconds(10),
            retryDelay: TimeSpan.FromMilliseconds(200));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => printer.ConnectAsync(cts.Token));
    }

    [Fact]
    public async Task SendRawDataAsync_ShouldThrow_WhenNotConnected()
    {
        using var printer = new PrinterNetworkService("127.0.0.1", 9100);

        await Assert.ThrowsAsync<InvalidOperationException>(() => printer.SendRawDataAsync(new byte[] { 0x00 }));
    }
}
