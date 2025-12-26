using Blanquita.Interfaces;

namespace Blanquita.Services
{
    public class PrinterNetworkServiceFactory : IPrinterNetworkServiceFactory
    {
        public IPrinterNetworkService Create(string ipAddress, int port = 9100)
        {
            return new PrinterNetworkService(ipAddress, port);
        }
    }
}
