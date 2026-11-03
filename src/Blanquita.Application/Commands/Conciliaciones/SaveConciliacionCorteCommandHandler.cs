using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Commands.Conciliaciones;

/// <summary>
/// Handler para el comando SaveConciliacionCorteCommand.
/// </summary>
public class SaveConciliacionCorteCommandHandler : IRequestHandler<SaveConciliacionCorteCommand, bool>
{
    private readonly IConciliacionService _conciliacionService;
    private readonly IFoxProShiftRepository _shiftRepository;
    private readonly ILogger<SaveConciliacionCorteCommandHandler> _logger;
    private readonly IConfiguracionService _configuracionService;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ISupervisorRepository _supervisorRepository;
    private readonly IBranchRepository _branchRepository;

    public SaveConciliacionCorteCommandHandler(
        IConciliacionService conciliacionService,
        IFoxProShiftRepository shiftRepository,
        ILogger<SaveConciliacionCorteCommandHandler> logger,
        IConfiguracionService configuracionService,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ISupervisorRepository supervisorRepository,
        IBranchRepository branchRepository)
    {
        _conciliacionService = conciliacionService;
        _shiftRepository = shiftRepository;
        _logger = logger;
        _configuracionService = configuracionService;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _supervisorRepository = supervisorRepository;
        _branchRepository = branchRepository;
    }

    public async Task<bool> Handle(SaveConciliacionCorteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Guardando conciliación para el turno {ShiftId} en la sucursal {BranchName}", request.ShiftId, request.BranchName);

        try
        {
            // Validar que el turno esté cerrado en CONTPAQi POS (Status == 1)
            var shift = await _shiftRepository.GetShiftDataAsync(request.ShiftId, cancellationToken);
            if (shift == null)
            {
                throw new InvalidOperationException($"El turno con ID {request.ShiftId} no existe en CONTPAQi POS.");
            }

            if (shift.Status != 1)
            {
                throw new InvalidOperationException($"No se puede guardar la conciliación de corte para el turno {request.ShiftId} porque no ha sido cerrado en CONTPAQi POS.");
            }

            // Ventas, devoluciones y recolecciones se vuelven a leer en el servidor; los de la pantalla solo se comparan.
            // Si cambiaron desde la consulta, el usuario cuadró contra datos viejos y debe volver a consultar.
            var totalRecolecciones = await _conciliacionService.GetTotalRecolectadoAsync(request.CashRegisterName, shift, cancellationToken);
            var totalSold = shift.TotalSold;
            var devoluciones = Math.Abs(shift.ReturnsTotal);
            var salidasEfectivo = request.Salidas?.Sum(s => s.Monto) ?? request.SalidasEfectivo;

            if (!SameAmount(totalRecolecciones, request.TotalRecolecciones) ||
                !SameAmount(totalSold, request.TotalSold) ||
                !SameAmount(devoluciones, Math.Abs(request.ReturnsTotal)) ||
                !SameAmount(salidasEfectivo, request.SalidasEfectivo))
            {
                _logger.LogWarning(
                    "Montos del turno {ShiftId} distintos a los de la pantalla. Recolecciones {Recolecciones}/{RecoPantalla}, vendido {Vendido}/{VendidoPantalla}, devoluciones {Devoluciones}/{DevPantalla}, salidas {Salidas}/{SalidasPantalla}",
                    request.ShiftId, totalRecolecciones, request.TotalRecolecciones, totalSold, request.TotalSold,
                    devoluciones, Math.Abs(request.ReturnsTotal), salidasEfectivo, request.SalidasEfectivo);

                throw new InvalidOperationException(
                    "Los montos del turno cambiaron desde que se consultaron (ventas, devoluciones o recolecciones). Vuelva a consultar el turno y cuadre de nuevo antes de guardar.");
            }

            // Fórmulas financieras centralizadas
            var totalEfectivo = totalRecolecciones + request.EfectivoEntregado + salidasEfectivo;
            var totalTarjetas = request.Banregio + request.Banbajio;
            var totalEntregado = totalEfectivo + totalTarjetas;
            var totalEsperado = totalSold - devoluciones;
            var diferencia = totalEntregado - totalEsperado;

            var dto = new ConciliacionCorteDto
            {
                AperturaId = request.ShiftId,
                Sucursal = request.BranchName,
                Caja = request.CashRegisterName,
                Cajero = request.CashierName,
                TotalRecolecciones = totalRecolecciones,
                EfectivoEntregado = request.EfectivoEntregado,
                SalidasEfectivo = salidasEfectivo,
                TotalEfectivo = totalEfectivo,
                Banregio = request.Banregio,
                Banbajio = request.Banbajio,
                TotalTarjetas = totalTarjetas,
                Devoluciones = devoluciones,
                TotalEntregado = totalEntregado,
                TotalEsperado = totalEsperado,
                Diferencia = diferencia,
                Fecha = request.Fecha,
                Usuario = request.Usuario,
                Salidas = request.Salidas ?? new(),
                Terminales = request.Terminales ?? new()
            };

            await _conciliacionService.SaveConciliacionCorteAsync(dto, cancellationToken);
            _logger.LogInformation("Conciliación de corte para turno {ShiftId} guardada con éxito en el handler", request.ShiftId);

            // Alertas por Descuadres Financieros (Fase 5 - 5.2)
            if (Math.Abs(diferencia) >= 50.00m)
            {
                _logger.LogWarning("Se detectó un descuadre financiero de {Diferencia:C} (umbral >= $50.00 MXN) para el turno {ShiftId}. Enviando alertas...", diferencia, request.ShiftId);

                // Buscar ID de sucursal
                int? branchId = null;
                try
                {
                    var branches = await _branchRepository.GetAllAsync(cancellationToken);
                    var branch = branches.FirstOrDefault(b => string.Equals(b.Name, request.BranchName, StringComparison.OrdinalIgnoreCase));
                    if (branch != null)
                    {
                        branchId = branch.Id;
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró una sucursal con el nombre '{BranchName}' para enviar alertas de descuadre por WhatsApp.", request.BranchName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al buscar la sucursal '{BranchName}' para alertas de descuadre.", request.BranchName);
                }

                // 1. Enviar Alertas por Correo
                try
                {
                    var config = await _configuracionService.ObtenerConfiguracionAsync();
                    if (config != null && !string.IsNullOrWhiteSpace(config.AlertEmails))
                    {
                        var emails = config.AlertEmails
                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e));

                        var subject = $"⚠️ Alerta de Descuadre Financiero - Sucursal: {request.BranchName}";
                        var body = $@"
<h3>Alerta de Descuadre Financiero Detectado</h3>
<p>Se ha registrado un descuadre en el corte del turno <strong>{request.ShiftId}</strong> que supera el límite de $50.00 MXN.</p>
<table>
    <tr><td><strong>Sucursal:</strong></td><td>{System.Net.WebUtility.HtmlEncode(request.BranchName)}</td></tr>
    <tr><td><strong>Caja:</strong></td><td>{System.Net.WebUtility.HtmlEncode(request.CashRegisterName)}</td></tr>
    <tr><td><strong>Cajero:</strong></td><td>{System.Net.WebUtility.HtmlEncode(request.CashierName)}</td></tr>
    <tr><td><strong>Total Entregado:</strong></td><td>{totalEntregado:C}</td></tr>
    <tr><td><strong>Total Esperado:</strong></td><td>{totalEsperado:C}</td></tr>
    <tr><td><strong>Diferencia:</strong></td><td><strong style='color:red;'>{diferencia:C}</strong></td></tr>
</table>
<br/>
<p>Por favor revise la conciliación correspondiente.</p>";

                        foreach (var email in emails)
                        {
                            try
                            {
                                await _emailService.SendEmailAsync(email, subject, body);
                                _logger.LogInformation("Alerta de descuadre enviada a {Email}", email);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al enviar correo de alerta a {Email}", email);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar el envío de alertas por correo.");
                }

                // 2. Enviar Alertas por WhatsApp
                if (branchId.HasValue)
                {
                    try
                    {
                        var supervisors = await _supervisorRepository.GetByBranchAsync(branchId.Value, cancellationToken);
                        var activeSupervisorsWithPhone = supervisors
                            .Where(s => s.IsActive && !string.IsNullOrWhiteSpace(s.PhoneNumber));

                        var waMessage = $"⚠️ *ALERTA DE DESCUADRE* ⚠️\n\n" +
                                        $"Se registró un descuadre de *{diferencia:C}* en el corte del turno *{request.ShiftId}*.\n\n" +
                                        $"*Detalles:*\n" +
                                        $"• Sucursal: {request.BranchName}\n" +
                                        $"• Caja: {request.CashRegisterName}\n" +
                                        $"• Cajero: {request.CashierName}\n" +
                                        $"• Total Entregado: {totalEntregado:C}\n" +
                                        $"• Total Esperado: {totalEsperado:C}";

                        foreach (var supervisor in activeSupervisorsWithPhone)
                        {
                            try
                            {
                                await _whatsAppService.SendMessageAsync(supervisor.PhoneNumber!, waMessage);
                                _logger.LogInformation("Alerta de descuadre por WhatsApp enviada al supervisor {SupervisorName} ({Phone})", supervisor.Name, supervisor.PhoneNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al enviar alerta por WhatsApp al supervisor {SupervisorName} ({Phone})", supervisor.Name, supervisor.PhoneNumber);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar el envío de alertas por WhatsApp.");
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el guardado de la conciliación para el turno {ShiftId}", request.ShiftId);
            throw;
        }
    }

    // La pantalla redondea a centavos; diferencias menores no cuentan como cambio
    private static bool SameAmount(decimal server, decimal screen) =>
        Math.Round(server, 2, MidpointRounding.AwayFromZero) == Math.Round(screen, 2, MidpointRounding.AwayFromZero);
}
