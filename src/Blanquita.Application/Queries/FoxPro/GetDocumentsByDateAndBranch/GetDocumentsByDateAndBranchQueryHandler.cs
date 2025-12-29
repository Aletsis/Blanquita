using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Queries.FoxPro.GetDocumentsByDateAndBranch;

/// <summary>
/// Handler para obtener documentos por fecha y sucursal desde FoxPro.
/// </summary>
public class GetDocumentsByDateAndBranchQueryHandler 
    : IRequestHandler<GetDocumentsByDateAndBranchQuery, IEnumerable<DocumentDto>>
{
    private readonly IFoxProDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentsByDateAndBranchQueryHandler> _logger;

    public GetDocumentsByDateAndBranchQueryHandler(
        IFoxProDocumentRepository documentRepository,
        ILogger<GetDocumentsByDateAndBranchQueryHandler> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentDto>> Handle(
        GetDocumentsByDateAndBranchQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Obteniendo documentos para fecha: {Date}, sucursal: {BranchId}", 
            request.Date, 
            request.BranchId);

        var documents = await _documentRepository.GetByDateAndBranchAsync(
            request.Date, 
            request.BranchId, 
            cancellationToken);

        _logger.LogInformation("Se encontraron {Count} documentos", documents.Count());

        return documents;
    }
}
