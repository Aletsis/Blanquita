using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Cajas;

/// <summary>
/// Resultado de procesar un corte de caja.
/// </summary>
public record ProcessCashCutResult(bool Success, CashCutDto? CashCut, bool PrintingSucceeded, string? Message);

/// <summary>
/// Comando para procesar un corte de caja y realizar su impresión en la ticketera física configurada.
/// </summary>
public record ProcessCashCutCommand(ProcessCashCutRequest Request) : IRequest<ProcessCashCutResult>;
