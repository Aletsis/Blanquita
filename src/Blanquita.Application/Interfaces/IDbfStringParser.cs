using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para parsear cadenas de texto que contienen referencias a documentos.
/// Utilizado para extraer información de documentos desde campos de texto de FoxPro.
/// </summary>
public interface IDbfStringParser
{
    /// <summary>
    /// Parsea una cadena de texto que contiene referencias a documentos.
    /// Formato esperado: IdDocumento(10) + Serie(20) + Folio(resto)
    /// </summary>
    /// <param name="cadenaDocumentos">Cadena de texto con documentos separados por saltos de línea</param>
    /// <returns>Lista de referencias a documentos parseadas</returns>
    List<DocumentoRefDto> ParsearDocumentos(string cadenaDocumentos);
}
