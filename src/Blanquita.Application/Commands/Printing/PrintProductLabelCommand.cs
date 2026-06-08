using MediatR;

namespace Blanquita.Application.Commands.Printing;

/// <summary>
/// Comando para imprimir una etiqueta de producto en una impresora Zebra.
/// </summary>
public record PrintProductLabelCommand(
    string ProductCode,
    string ProductName,
    decimal Price,
    int PrinterId,
    int Quantity
) : IRequest<bool>;
