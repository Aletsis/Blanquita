using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Conciliaciones;

/// <summary>
/// Comando para guardar la conciliación de un corte de caja, encapsulando cálculos financieros.
/// </summary>
public record SaveConciliacionCorteCommand(
    int ShiftId,
    string BranchName,
    string CashRegisterName,
    string CashierName,
    decimal TotalRecolecciones,
    decimal EfectivoEntregado,
    decimal Banregio,
    decimal Banbajio,
    decimal ReturnsTotal,
    decimal TotalSold,
    DateTime Fecha,
    decimal SalidasEfectivo = 0,
    string? Usuario = null,
    List<ConciliacionSalidaEfectivoDto>? Salidas = null,
    List<TerminalDetalleDto>? Terminales = null
) : IRequest<bool>;
