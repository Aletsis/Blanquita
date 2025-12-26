using Blanquita.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Blanquita.Services.Parsing
{
    public class DbfStringParser : IDbfStringParser
    {
        private readonly ILogger<DbfStringParser> _logger;

        public DbfStringParser(ILogger<DbfStringParser> logger)
        {
            _logger = logger;
        }

        public List<DocumentoRef> ParsearDocumentos(string cadenaDocumentos)
        {
            var documentos = new List<DocumentoRef>();

            if (string.IsNullOrWhiteSpace(cadenaDocumentos))
                return documentos;

            try
            {
                _logger.LogDebug("    Parseando cadena (length={Length}): '{Cadena}'", cadenaDocumentos.Length, cadenaDocumentos);

                // Ejemplo de formato esperado: "4          FGIFS                 4821"
                // Donde: IdDocumento(10) + Serie(20) + Folio(resto)

                // Primero intentar como una sola línea
                if (!cadenaDocumentos.Contains('\n') && !cadenaDocumentos.Contains('\r'))
                {
                    if (cadenaDocumentos.Length >= 30)
                    {
                        var idDoc = cadenaDocumentos.Substring(0, 10).Trim();
                        var serie = cadenaDocumentos.Substring(10, 20).Trim();
                        var folio = cadenaDocumentos.Substring(30).Trim();

                        _logger.LogDebug("    Parseado: ID='{IdDoc}', Serie='{Serie}', Folio='{Folio}'", idDoc, serie, folio);

                        if (!string.IsNullOrEmpty(idDoc) && !string.IsNullOrEmpty(serie) && !string.IsNullOrEmpty(folio))
                        {
                            documentos.Add(new DocumentoRef
                            {
                                IdDocumento = idDoc,
                                Serie = serie,
                                Folio = folio
                            });
                        }
                    }
                    else
                    {
                        _logger.LogDebug("    Cadena muy corta (< 30 caracteres)");
                    }
                }
                else
                {
                    // Si tiene saltos de línea, procesar cada línea
                    var partes = cadenaDocumentos.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    _logger.LogDebug("    Encontradas {Count} líneas", partes.Length);

                    foreach (var parte in partes)
                    {
                        if (parte.Length < 30) continue;

                        var idDoc = parte.Substring(0, 10).Trim();
                        var serie = parte.Substring(10, 20).Trim();
                        var folio = parte.Substring(30).Trim();

                        if (!string.IsNullOrEmpty(idDoc) && !string.IsNullOrEmpty(serie) && !string.IsNullOrEmpty(folio))
                        {
                            documentos.Add(new DocumentoRef
                            {
                                IdDocumento = idDoc,
                                Serie = serie,
                                Folio = folio
                            });
                            _logger.LogDebug("      Línea parseada: ID='{IdDoc}', Serie='{Serie}', Folio='{Folio}'", idDoc, serie, folio);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Error al parsear documentos: {Message}", ex.Message);
                _logger.LogDebug("Cadena problemática: '{CadenaDocumentos}'", cadenaDocumentos);
            }

            _logger.LogDebug("    Total documentos parseados: {Count}", documentos.Count);
            return documentos;
        }
    }
}
