using MediatR;
using System;

namespace Blanquita.Application.Queries.Cajas;

/// <summary>
/// Query para obtener el total acumulado en efectivo de las recolecciones realizadas hoy (sin corte) para una caja específica.
/// </summary>
public record GetCollectionsTotalQuery(string CashRegisterName, DateTime Date) : IRequest<decimal>;
