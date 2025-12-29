using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceder a documentos desde FoxPro.
/// </summary>
public interface IFoxProDocumentRepository
{
    /// <summary>
    /// Obtiene documentos por fecha y sucursal.
    /// </summary>
    /// <param name="date">Fecha de los documentos</param>
    /// <param name="branchId">ID de la sucursal</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de documentos</returns>
    Task<IEnumerable<DocumentDto>> GetByDateAndBranchAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default);
}
