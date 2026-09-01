using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetOpenPedidosReport;

public record GetOpenPedidosReportQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    IEnumerable<string>? Series = null,
    string? Comanda = null,
    string? Ruta = null,
    string? SucursalNombre = null
) : IRequest<IEnumerable<OpenPedidoReportItemDto>>;
