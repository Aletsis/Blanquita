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
        const int perAttemptTimeoutMs = 5000;
        const int totalTimeoutSec = 30; //Tiempo maximo global en segundos
        using var totalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        totalCts.CancelAfter(TimeSpan.FromSeconds(totalTimeoutSec));

        while (!totalCts.Token.IsCancellationRequested)
        {
            _client?.Dispose();
            _client = new TcpClient();

            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(totalCts.Token);
            attemptCts.CancelAfter(perAttemptTimeoutMs);

            try
            {
                await _client.ConnectAsync(_ipAddress, _port, attemptCts.Token);
                _stream = _client.GetStream();
                return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (totalCts.Token.IsCancellationRequested) throw new TimeoutException($"No se pudo conectar a la impresora {_ipAddress}:{_port} en {totalTimeoutSec} segundos.");
            }
            catch (Exception) when (!totalCts.Token.IsCancellationRequested)
            {
                //Reintentamos otros errores
            }
            //Si se canceló el total, salimos del bucle y lanzamos excepción
        }
        //Si salimos del bucle por cancelación total
        throw new TimeoutException ($"No se pudo conectar a {_ipAddress}:{_port} en {totalTimeoutSec} segundos.");
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
