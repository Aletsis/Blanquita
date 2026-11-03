using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Conciliaciones;

/// <summary>
/// Comando para actualizar/editar una conciliación de corte existente.
/// <paramref name="Version"/> es la versión con la que se cargó; si otro usuario guardó antes, la edición se rechaza.
/// </summary>
public record UpdateConciliacionCorteCommand(
    int Id,
    uint Version,
    decimal EfectivoEntregado,
    decimal SalidasEfectivo,
    decimal Banregio,
    decimal Banbajio,
    string? ModificadoPor = null,
    List<ConciliacionSalidaEfectivoDto>? Salidas = null,
    List<TerminalDetalleDto>? Terminales = null
) : IRequest<bool>;
