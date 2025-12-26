using System.Net.Sockets;
using Blanquita.Interfaces;

namespace Blanquita.Services
{
    public class PrinterNetworkService : IPrinterNetworkService
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly string _ipAddress;
        private readonly int _port;
        private bool _disposed;
        public PrinterNetworkService(string ipAddress, int port = 9100)
        {
            _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            _port = port;
        }
        public bool IsConnected => _tcpClient?.Connected ?? false;
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PrinterNetworkService));
            if (_tcpClient?.Connected == true) return;

            _tcpClient = new TcpClient();
            try
            {
                using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectCts.CancelAfter(TimeSpan.FromSeconds(15)); // Timeout opcional
                await _tcpClient.ConnectAsync(_ipAddress, _port, connectCts.Token);

                _networkStream = _tcpClient.GetStream();
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Se agotó el tiempo al intentar conectar con la impresora.");
            }
            catch (SocketException ex)
            {
                throw new IOException("No se pudo conectar con la impresora de red.", ex);
            }
        }

        public async Task SendRawDataAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PrinterNetworkService));

            if (!IsConnected || _networkStream == null)
                throw new InvalidOperationException("La conexión con la impresora no está activa.");

            if (data == null || data.Length == 0)
                throw new ArgumentException("No se proporcionaron datos para imprimir.", nameof(data));

            try
            {
                await _networkStream.WriteAsync(data, 0, data.Length, cancellationToken);
                await _networkStream.FlushAsync(cancellationToken);
            }
            catch (IOException ex)
            {
                throw new IOException("Error al enviar datos a la impresora.", ex);
            }
        }

        public void Disconnect()
        {
            if (_disposed) return;

            _networkStream?.Close();
            _tcpClient?.Close();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _networkStream?.Dispose();
                _tcpClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
