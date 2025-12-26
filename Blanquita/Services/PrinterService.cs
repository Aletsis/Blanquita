using Blanquita.Data;
using Blanquita.Models;
using Blanquita.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Services
{
    public class PrinterService
    {
        private readonly List<PrinterConfiguration> _printers;
        private PrinterConfiguration _selectedPrinter;
        private readonly Context _context;
        private Cajas _impresoraSeleccionada;
        private readonly ILogger<PrinterService> _logger;
        private readonly IPrinterCommandBuilder _commandBuilder;
        private readonly IPrinterNetworkServiceFactory _networkServiceFactory;

        public PrinterService(Context context, ILogger<PrinterService> logger, IPrinterCommandBuilder commandBuilder, IPrinterNetworkServiceFactory networkServiceFactory)
        {
            _context = context;
            _logger = logger;
            _commandBuilder = commandBuilder;
            _networkServiceFactory = networkServiceFactory;
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
            return await _context.Cajas.FirstOrDefaultAsync(c => c.Id == idImpresora);
        }

        public void SelectPrinter(int printerId)
        {
            _selectedPrinter = _printers.Find(p => p.Id == printerId) ??
                throw new ArgumentException("Impresora no encontrada");
        }

        public async Task PrintTicketAsync(int cantMil, int cantQuinientos, int cantDoscientos, int cantCien, int cantCincuenta, int cantVeinte, int ipImpresora, string caja, string cajera, string encargada, int folio, string sucursal)
        {
            string targetIp;
            int targetPort = 9100;

            if (_selectedPrinter != null)
            {
                targetIp = _selectedPrinter.IpAddress;
            }
            else
            {
                _impresoraSeleccionada = await SeleccionaImpresora(ipImpresora);
                if (_impresoraSeleccionada == null)
                {
                    throw new InvalidOperationException("No se ha seleccionado ninguna impresora");
                }
                targetIp = _impresoraSeleccionada.IpImpresora;
                targetPort = _impresoraSeleccionada.Port;
                _logger.LogInformation("Impresora seleccionada con IP: {IpImpresora}", targetIp);
            }

            var ticketData = new PrintTicketData
            {
                CantMil = cantMil,
                CantQuinientos = cantQuinientos,
                CantDoscientos = cantDoscientos,
                CantCien = cantCien,
                CantCincuenta = cantCincuenta,
                CantVeinte = cantVeinte,
                Caja = caja,
                Cajera = cajera,
                Encargada = encargada,
                Folio = folio,
                Sucursal = sucursal
            };

            var printCommands = _commandBuilder.BuildTicket(ticketData);
            await SendToPrinterAsync(targetIp, targetPort, printCommands.ToArray());
        }

        public async Task ImprimirCorte(int totalM, int totalQ, int totalD, int totalC, int totalCi, int totalV, int ipImpresora, decimal totalTira, decimal totalTarjetas, int granTotal, string caja, string encargada, string cajera, string sucursal)
        {
            string targetIp;
            int targetPort = 9100;

            if (_selectedPrinter != null)
            {
                targetIp = _selectedPrinter.IpAddress;
            }
            else
            {
                _impresoraSeleccionada = await SeleccionaImpresora(ipImpresora);
                if (_impresoraSeleccionada == null)
                {
                    throw new InvalidOperationException("No se ha seleccionado ninguna impresora");
                }
                targetIp = _impresoraSeleccionada.IpImpresora;
                targetPort = _impresoraSeleccionada.Port;
            }

            var corteData = new PrintCorteData
            {
                TotalM = totalM,
                TotalQ = totalQ,
                TotalD = totalD,
                TotalC = totalC,
                TotalCi = totalCi,
                TotalV = totalV,
                TotalTira = totalTira,
                TotalTarjetas = totalTarjetas,
                GranTotal = granTotal,
                Caja = caja,
                Encargada = encargada,
                Cajera = cajera,
                Sucursal = sucursal,
                FechaHora = DateTime.Now
            };

            var printCommands = _commandBuilder.BuildCorteCaja(corteData);
            await SendToPrinterAsync(targetIp, targetPort, printCommands.ToArray());
        }

        private async Task SendToPrinterAsync(string ipAddress, int port, byte[] data)
        {
            _logger.LogDebug("Enviando comandos a impresora: {IpImpresora}:{Port}", ipAddress, port);
            using var printerNetworkService = _networkServiceFactory.Create(ipAddress, port);

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
    }
}
