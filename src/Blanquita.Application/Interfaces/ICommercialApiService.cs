using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICommercialApiService
{
    Task<IEnumerable<CommercialCompraDto>> GetComprasAsync(
        string? serie, 
        double? folio, 
        int? proveedorId, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta, 
        string username,
        int? limit = null);
        
    Task<IEnumerable<CommercialMovimientoDto>> GetMovimientosAsync(int documentoId, string username);
    
    Task<IEnumerable<CommercialProveedorDto>> SearchProveedoresAsync(string? searchTerm, string username, int pageSize = 50);
    
    /// <summary>Busca productos; devuelve como máximo <paramref name="pageSize"/> resultados.</summary>
    Task<IEnumerable<CommercialProductoDto>> GetProductosAsync(string? searchTerm, string username, bool onlyActive = true, int pageSize = 50);
    
    Task<CommercialProductoDto?> GetProductoByIdAsync(int id, string username);

    /// <summary>
    /// Consulta solo los productos indicados (p. ej. los de las partidas de un documento), en lugar de todo el catálogo.
    /// Los que no se encuentran o fallan se omiten del resultado.
    /// </summary>
    Task<IReadOnlyDictionary<int, CommercialProductoDto>> GetProductosByIdsAsync(IEnumerable<int> ids, string username, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<CommercialSalidaDto>> GetSalidasAsync(
        string? serie, 
        double? folio, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta, 
        string username,
        IEnumerable<string>? allowedConcepts = null);

    Task<byte[]> GetSalidaPdfAsync(
        string? codigoConcepto, 
        string? serie, 
        double folio, 
        string username);

    Task<IEnumerable<CommercialConceptoDto>> GetConceptosAsync(
        int tipoDocumento, 
        string username);

    Task<CreateCommercialSalidaResponseDto?> CreateSalidaAsync(CreateCommercialSalidaDto dto, string username);
}



