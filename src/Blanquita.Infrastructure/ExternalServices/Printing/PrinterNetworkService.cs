using System.Net.Sockets;

namespace Blanquita.Infrastructure.ExternalServices.Printing;

public class PrinterNetworkService : IDisposable
{
    /// <summary>Tiempo máximo para cada intento individual de conexión.</summary>
    public static readonly TimeSpan DefaultAttemptTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Tiempo máximo total para conectar, incluyendo reintentos.</summary>
    public static readonly TimeSpan DefaultTotalTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Pausa entre reintentos, para no saturar la red ni el CPU cuando la impresora rechaza la conexión.</summary>
    public static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(1);

    /// <summary>Tiempo máximo para enviar los datos una vez conectados.</summary>
    public static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(10);

    private readonly string _ipAddress;
    private readonly int _port;
    private readonly TimeSpan _attemptTimeout;
    private readonly TimeSpan _totalTimeout;
    private readonly TimeSpan _retryDelay;
    private readonly TimeSpan _sendTimeout;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _disposed;

    public PrinterNetworkService(
        string ipAddress,
        int port,
        TimeSpan? attemptTimeout = null,
        TimeSpan? totalTimeout = null,
        TimeSpan? retryDelay = null,
        TimeSpan? sendTimeout = null)
    {
        _ipAddress = ipAddress;
        _port = port;
        _attemptTimeout = attemptTimeout ?? DefaultAttemptTimeout;
        _totalTimeout = totalTimeout ?? DefaultTotalTimeout;
        _retryDelay = retryDelay ?? DefaultRetryDelay;
        _sendTimeout = sendTimeout ?? DefaultSendTimeout;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        using var totalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        totalCts.CancelAfter(_totalTimeout);

        Exception? lastError = null;

        while (true)
        {
            _client?.Dispose();
            _client = new TcpClient();

            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(totalCts.Token);
            attemptCts.CancelAfter(_attemptTimeout);

            try
            {
                await _client.ConnectAsync(_ipAddress, _port, attemptCts.Token);
                _stream = _client.GetStream();
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Cancelación solicitada por quien llama: no es un timeout
                throw;
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException or IOException)
            {
                // Timeout del intento o error de red (p. ej. conexión rechazada): se reintenta
                lastError = ex;
            }

            try
            {
                await Task.Delay(_retryDelay, totalCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Se agotó el tiempo total durante la pausa
                break;
            }

            if (totalCts.Token.IsCancellationRequested)
            {
                break;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new TimeoutException(
            $"No se pudo conectar a la impresora {_ipAddress}:{_port} en {_totalTimeout.TotalSeconds:0} segundos.",
            lastError);
    }

    public async Task SendRawDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (_stream == null)
            throw new InvalidOperationException("Not connected to printer");

        using var sendCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        sendCts.CancelAfter(_sendTimeout);

        try
        {
            await _stream.WriteAsync(data, sendCts.Token);
            await _stream.FlushAsync(sendCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"La impresora {_ipAddress}:{_port} no aceptó los datos en {_sendTimeout.TotalSeconds:0} segundos.");
        }
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
