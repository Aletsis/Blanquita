namespace Blanquita.Application.DTOs;

/// <summary>
/// Request to get available printers for reprinting a document
/// </summary>
public record GetPrintersForReprintRequest
{
    /// <summary>
    /// ID of the document to reprint (CashCollection or CashCut)
    /// </summary>
    public int DocumentId { get; init; }
    
    /// <summary>
    /// Type of document to reprint
    /// </summary>
    public ReprintDocumentType DocumentType { get; init; }
}

/// <summary>
/// Type of document that can be reprinted
/// </summary>
public enum ReprintDocumentType
{
    CashCollection,
    CashCut
}
