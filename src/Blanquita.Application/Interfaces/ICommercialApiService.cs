using System;
using System.Collections.Generic;
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
        string username);
        
    Task<IEnumerable<CommercialMovimientoDto>> GetMovimientosAsync(int documentoId, string username);
    
    Task<IEnumerable<CommercialProveedorDto>> SearchProveedoresAsync(string? searchTerm, string username, int pageSize = 50);
    
    Task<IEnumerable<CommercialProductoDto>> GetProductosAsync(string? searchTerm, string username, bool onlyActive = true);
    
    Task<CommercialProductoDto?> GetProductoByIdAsync(int id, string username);
    
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



