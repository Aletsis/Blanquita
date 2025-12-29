using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetDocumentsByDateAndBranch;

/// <summary>
/// Query para obtener documentos por fecha y sucursal desde FoxPro.
/// </summary>
public record GetDocumentsByDateAndBranchQuery(DateTime Date, int BranchId) : IRequest<IEnumerable<DocumentDto>>;
