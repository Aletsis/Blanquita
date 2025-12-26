using Microsoft.JSInterop;
using System.Net.Sockets;
using System.Net;
using Blanquita.Models;
using Blanquita.Interfaces;

namespace Blanquita.Services
{
    public class ZebraPrinterService : IZebraPrinterService
    {
        private readonly ILogger<ZebraPrinterService> _logger;

        public ZebraPrinterService(ILogger<ZebraPrinterService> logger)
        {
            _logger = logger;
        }

        public async Task PrintViaNetwork(string ipAddress, int port, string zplContent)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(IPAddress.Parse(ipAddress), port);
                    using (var stream = client.GetStream())
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            await writer.WriteAsync(zplContent);
                            await writer.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al imprimir en {IpAddress}:{Port}", ipAddress, port);
            }
        }
        public async Task PrintViaBrowser(IJSRuntime jSRuntime, string htmlContent)
        {
            try
            {
                await jSRuntime.InvokeVoidAsync("printHtmlContent", htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de impresion via navegador");
            }
        }
        public string GenerateLabel(LabelDesign design, string title, string barcode, string productPrice, string footer, int cantidad)
        {
            return $@"^XA^PQ{cantidad}
                    ^PW{design.WidthInDots}
                    ^LL{design.HeightInDots}
                    ^LS{design.MarginTop}
                    ^FO{design.MarginLeft},30^A0N,30,30^FD{title}^FS
                    ^FO{design.MarginLeft},80^BY2^BCN,70,Y,N,N^FD{barcode}^FS
                    ^FO440,80^A0N,120,55^FD${productPrice}^FS
                    ^FO{design.MarginLeft},180^A0N,20,20^FD{footer}^FS
                    ^XZ";
        }
    }
}
