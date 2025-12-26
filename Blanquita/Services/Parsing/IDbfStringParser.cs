using Blanquita.Models;
using System.Collections.Generic;

namespace Blanquita.Services.Parsing
{
    public interface IDbfStringParser
    {
        List<DocumentoRef> ParsearDocumentos(string cadenaDocumentos);
    }
}
