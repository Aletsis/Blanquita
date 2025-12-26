using Blanquita.Interfaces;
using Blanquita.Models;

namespace Blanquita.Services
{
    public class PrintJobService
    {
        private readonly IPrinterRepository _printerRepo;
        private readonly IPrinterCommandBuilder _commandBuilder;
        private readonly IPrinterNetworkServiceFactory _printerFactory;
        private readonly ILogger<PrintJobService> _logger;
        public PrintJobService(
            IPrinterRepository printerRepo,
            IPrinterCommandBuilder commandBuilder,
            IPrinterNetworkServiceFactory printerFactory,
            ILogger<PrintJobService> logger)
        {
            _printerRepo = printerRepo ?? throw new ArgumentNullException(nameof(printerRepo));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _printerFactory = printerFactory ?? throw new ArgumentNullException(nameof(printerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task PrintTicketAsync(int cajaId, PrintTicketData data)
        {
            var caja = await _printerRepo.GetCajaByIdAsync(cajaId);
            if (caja == null)
                throw new ArgumentException($"Caja con ID {cajaId} no encontrada.");

            var cmds = _commandBuilder.BuildTicket(data);
            await SendToPrinterAsync(caja, cmds.ToArray(), "ticket");
        }
        public async Task PrintCorteCajaAsync(int cajaId, PrintCorteData data)
        {
            var caja = await _printerRepo.GetCajaByIdAsync(cajaId);
            if (caja == null)
                throw new ArgumentException($"Caja con ID {cajaId} no encontrada.");

            var cmds = _commandBuilder.BuildCorteCaja(data);
            await SendToPrinterAsync(caja, cmds.ToArray(), "corte de caja");
        }
        private async Task SendToPrinterAsync(Cajas caja, byte[] data, string tipo)
        {
            using var printer = _printerFactory.Create(caja.IpImpresora, caja.Port);

            try
            {
                await printer.ConnectAsync();
                await printer.SendRawDataAsync(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar {tipo} a la impresora {caja.IpImpresora} (Caja {caja.Id})");
                throw;
            }
            finally
            {
                printer.Disconnect(); // también llamado en Dispose, pero por claridad lo dejamos
            }
        }
    }
}
