namespace Blanquita.Interfaces
{
    public interface IPrinterNetworkService : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task SendRawDataAsync(byte[] data, CancellationToken cancellationToken = default);
        void Disconnect();
    }
}
