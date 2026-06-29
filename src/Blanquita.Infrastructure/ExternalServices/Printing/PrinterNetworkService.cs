using System.Net.Sockets;

namespace Blanquita.Infrastructure.ExternalServices.Printing;

public class PrinterNetworkService : IDisposable
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _disposed;

    public PrinterNetworkService(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(3000);

        try
        {
            await _client.ConnectAsync(_ipAddress, _port, cts.Token);
            _stream = _client.GetStream();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"No se pudo conectar a la impresora {_ipAddress}:{_port} dentro del límite de 3 segundos.");
        }
    }

    public async Task SendRawDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (_stream == null)
            throw new InvalidOperationException("Not connected to printer");

        await _stream.WriteAsync(data, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Disconnect();
        _stream?.Dispose();
        _client?.Dispose();
        _disposed = true;
    }
}
