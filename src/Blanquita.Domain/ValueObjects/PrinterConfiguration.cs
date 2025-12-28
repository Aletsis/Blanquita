using System.Net;

namespace Blanquita.Domain.ValueObjects;

public record PrinterConfiguration
{
    public string IpAddress { get; }
    public int Port { get; }

    // Constructor requerido por EF Core
    private PrinterConfiguration() { }

    private PrinterConfiguration(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    public static PrinterConfiguration Create(string ipAddress, int port)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

        if (!IPAddress.TryParse(ipAddress, out _))
            throw new ArgumentException("Invalid IP address format", nameof(ipAddress));

        if (port <= 0 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(port));

        return new PrinterConfiguration(ipAddress, port);
    }

    public override string ToString() => $"{IpAddress}:{Port}";
}
