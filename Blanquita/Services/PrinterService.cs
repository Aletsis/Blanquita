using Blanquita.Data;
using Blanquita.Models;
using Microsoft.JSInterop;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Services
{
    public class PrinterService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly List<PrinterConfiguration> _printers;
        private PrinterConfiguration _selectedPrinter;

        private readonly Context _context;
        private readonly List<Cajas> _impresoras;
        private Cajas _impresoraSeleccionada;

        private readonly ILogger<PrinterService> _logger;

        public PrinterService(IJSRuntime jsRuntime, Context context, ILogger<PrinterService> logger)
        {
            _jsRuntime = jsRuntime;
            _context = context;
            _logger = logger;
            _printers = new List<PrinterConfiguration>
        {
            new PrinterConfiguration { Id = 1, Name = "Impresora de Prueba 1", IpAddress = "192.168.1.170" },
            new PrinterConfiguration { Id = 2, Name = "Impresora de Prueba 2", IpAddress = "192.168.1.21" },
            new PrinterConfiguration { Id = 3, Name = "Impresora de Prueba 3", IpAddress = "192.168.1.22" },
            new PrinterConfiguration { Id = 4, Name = "Impresora de Prueba 4", IpAddress = "192.168.1.23" },
            new PrinterConfiguration { Id = 5, Name = "Impresora de Prueba 5", IpAddress = "192.168.1.24" },
            new PrinterConfiguration { Id = 6, Name = "Impresora de Prueba 6", IpAddress = "192.168.1.25" },
            new PrinterConfiguration { Id = 7, Name = "Impresora de Prueba 7", IpAddress = "192.168.1.26" }
        };
        }

        public IEnumerable<PrinterConfiguration> GetAvailablePrinters() => _printers;
        public async Task<Cajas> SeleccionaImpresora(int idImpresora)
        {
            _logger.LogDebug("Buscando impresora con id: {IdImpresora}", idImpresora);
            var caja = await _context.Cajas.FirstOrDefaultAsync(c => c.Id == idImpresora);
            return caja;
        }
        public void SelectPrinter(int printerId)
        {
            _selectedPrinter = _printers.Find(p => p.Id == printerId) ??
                throw new ArgumentException("Impresora no encontrada");
        }

        public async Task PrintTicketAsync(int cantMil, int cantQuinientos, int cantDoscientos, int cantCien, int cantCincuenta, int cantVeinte, int ipImpresora, string caja, string cajera, string encargada, int folio, string sucursal)
        {
            _impresoraSeleccionada = await SeleccionaImpresora(ipImpresora);
            _logger.LogInformation("Impresora seleccionada con IP: {IpImpresora}", _impresoraSeleccionada.IpImpresora);
            if (_selectedPrinter == null && _impresoraSeleccionada == null)
            {
                throw new InvalidOperationException("No se ha seleccionado ninguna impresora");
            }
            var total = 0;
            // Crear comandos ESC/POS directamente
            var printCommands = new List<byte>();

            // Enviar comandos directamente a la impresora
            printCommands.AddRange(InitializePrinter());
            // Encabezado
            printCommands.AddRange(AlignCenter());
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
            printCommands.AddRange(BoldOff());
            printCommands.AddRange(Text("RECOLECCION DE EFECTIVO\n"));
            printCommands.AddRange(Text($"{sucursal}\n\n"));

            // Alinear izquierda
            printCommands.AddRange(AlignLeft());
            printCommands.AddRange(Text("Fecha: " + DateTime.Now.ToString("g") + "\n"));
            printCommands.AddRange(Text($"{caja}\n"));
            printCommands.AddRange(Text($"Recoleccion {folio}\n"));
            printCommands.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL\n"));
            printCommands.AddRange(AlignLeft());
            printCommands.AddRange(Text("------------------------------------------\n"));

            // Items
            printCommands.AddRange(Text($"$1000                {cantMil}          ${cantMil * 1000}.00\n"));
            total += cantMil * 1000;
            printCommands.AddRange(Text($"$500                 {cantQuinientos}          ${cantQuinientos * 500}.00\n"));
            total += cantQuinientos * 500;
            printCommands.AddRange(Text($"$200                 {cantDoscientos}          ${cantDoscientos * 200}.00\n"));
            total += cantDoscientos * 200;
            printCommands.AddRange(Text($"$100                 {cantCien}          ${cantCien * 100}.00\n"));
            total += cantCien * 100;
            printCommands.AddRange(Text($"$50                  {cantCincuenta}          ${cantCincuenta * 50}.00\n"));
            total += cantCincuenta * 50;
            printCommands.AddRange(Text($"$20                  {cantVeinte}          ${cantVeinte * 20}.00\n"));
            total += cantVeinte * 20;
            printCommands.AddRange(Text("----------------------------------------\n"));

            // Total
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text($"TOTAL:            ${total}.00\n\n\n\n"));
            printCommands.AddRange(BoldOff());
            printCommands.AddRange(Text("---------------             --------------\n"));
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text($"{(encargada.Length > 15 ? encargada.Substring(0, 15) : encargada.PadRight(15))}             {(cajera.Length > 14 ? cajera.Substring(0, 14) : cajera.PadRight(14))}\n\n\n"));

            // Cortar papel
            printCommands.AddRange(CutPaper());
            await SendToPrinterAsync(printCommands.ToArray());
        }
        public async Task ImprimirCorte(int totalM, int totalQ, int totalD, int totalC, int totalCi, int totalV, int ipImpresora, decimal totalTira, decimal totalTarjetas, int granTotal, string caja, string encargada, string cajera, string sucursal)
        {
            _impresoraSeleccionada = await SeleccionaImpresora(ipImpresora);
            if (_selectedPrinter == null && _impresoraSeleccionada == null)
            {
                throw new InvalidOperationException("No se ha seleccionado ninguna impresora");
            }
            var printCommands = new List<byte>();
            printCommands.AddRange(InitializePrinter());
            printCommands.AddRange(AlignCenter());
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
            printCommands.AddRange(BoldOff());
            printCommands.AddRange(Text("CORTE DE CAJA\n"));
            printCommands.AddRange(Text($"{sucursal}\n\n"));
            printCommands.AddRange(AlignLeft());
            printCommands.AddRange(Text("Fecha: " + DateTime.Now.ToString("g") + "\n"));
            printCommands.AddRange(Text($"{caja}\n"));
            printCommands.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL \n"));
            printCommands.AddRange(AlignLeft());
            printCommands.AddRange(Text("------------------------------------------\n"));
            printCommands.AddRange(Text($"$1000              {totalM}           ${totalM * 1000}.00\n"));
            printCommands.AddRange(Text($"$500               {totalQ}           ${totalQ * 500}.00\n"));
            printCommands.AddRange(Text($"$200               {totalD}           ${totalD * 200}.00\n"));
            printCommands.AddRange(Text($"$100               {totalC}           ${totalC * 100}.00\n"));
            printCommands.AddRange(Text($"$50                {totalCi}           ${totalCi * 50}.00\n"));
            printCommands.AddRange(Text($"$20                {totalV}           ${totalV * 20}.00\n"));
            printCommands.AddRange(Text("----------------------------------------\n"));
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text($"TOTAL RECOLECCIONES:          ${granTotal}.00\n"));
            printCommands.AddRange(Text($"TOTAL TIRA:                   ${totalTira}\n"));
            printCommands.AddRange(Text($"TOTAL TARJETAS:               ${totalTarjetas}\n\n"));
            printCommands.AddRange(Text($"EFECTIVO A ENTREGAR:          ${totalTira - (totalTarjetas + granTotal)}\n\n\n\n"));
            printCommands.AddRange(BoldOff());
            printCommands.AddRange(Text("---------------            ---------------\n"));
            printCommands.AddRange(BoldOn());
            printCommands.AddRange(Text($"{(encargada.Length > 15 ? encargada.Substring(0, 15) : encargada.PadRight(15))}             {(cajera.Length > 14 ? cajera.Substring(0, 14) : cajera.PadRight(14))}\n\n\n"));
            printCommands.AddRange(CutPaper());
            await SendToPrinterAsync(printCommands.ToArray());
        }

        private async Task SendToPrinterAsync(byte[] data)
        {
            _logger.LogDebug("Enviando comandos a impresora: {IpImpresora}", _impresoraSeleccionada.IpImpresora);
            using var printerNetworkService = new PrinterNetworkService(
                _impresoraSeleccionada.IpImpresora,
                _impresoraSeleccionada.Port);

            try
            {
                await printerNetworkService.ConnectAsync();
                await printerNetworkService.SendRawDataAsync(data);
            }
            finally
            {
                printerNetworkService.Disconnect();
            }
        }
        // Inicializar impresora
        public static byte[] InitializePrinter() => new byte[] { 0x1B, 0x40 };

        // Cortar papel
        public static byte[] CutPaper() => new byte[] { 0x1D, 0x56, 66, 0 };

        // Texto normal
        public static byte[] Text(string text) => Encoding.ASCII.GetBytes(text);

        // Texto en negrita (ON)
        public static byte[] BoldOn() => new byte[] { 0x1B, 0x45, 1 };

        // Texto en negrita (OFF)
        public static byte[] BoldOff() => new byte[] { 0x1B, 0x45, 0 };

        // Alineación
        public static byte[] AlignLeft() => new byte[] { 0x1B, 0x61, 0 };
        public static byte[] AlignCenter() => new byte[] { 0x1B, 0x61, 1 };
        public static byte[] AlignRight() => new byte[] { 0x1B, 0x61, 2 };
    }
}
