namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO que representa una referencia a un documento en el sistema FoxPro.
/// Utilizado para parsear y referenciar documentos desde campos de texto.
/// </summary>
public class DocumentoRefDto
{
    public string IdDocumento { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
}
