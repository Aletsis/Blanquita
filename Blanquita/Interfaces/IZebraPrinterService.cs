using Blanquita.Models;
using Microsoft.JSInterop;

namespace Blanquita.Interfaces
{
    public interface IZebraPrinterService
    {
        Task PrintViaNetwork(string ipAddress, int port, string zplContent);
        Task PrintViaBrowser(IJSRuntime jSRuntime, string htmlContent);
        string GenerateLabel(LabelDesign design, string title, string barcode, string productPrice, string footer, int cantidad);
    }
}
