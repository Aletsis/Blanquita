namespace Blanquita.Interfaces
{
    public interface IPrinterNetworkServiceFactory
    {
        IPrinterNetworkService Create(string ipAddress, int port = 9100);
    }
}
