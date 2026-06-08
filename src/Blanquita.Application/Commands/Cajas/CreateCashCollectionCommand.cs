using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Cajas;

/// <summary>
/// Resultado de crear una recolección de efectivo.
/// </summary>
public record CreateCashCollectionResult(bool Success, CashCollectionDto? CashCollection, bool PrintingSucceeded, bool UsedBackupPrinter, string? Message);

/// <summary>
/// Comando para registrar una recolección física de efectivo y realizar su impresión en la ticketera física (con soporte para impresora de respaldo).
/// </summary>
public record CreateCashCollectionCommand(CreateCashCollectionDto Dto, int CashRegisterId) : IRequest<CreateCashCollectionResult>;
