namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para búsqueda paginada de cajeras con filtros específicos.
/// </summary>
public sealed class SearchCashierRequest : PagedSearchRequest
{
    /// <summary>
    /// Filtrar por ID de sucursal (opcional)
    /// </summary>
    public int? BranchId { get; init; }

    /// <summary>
    /// Filtrar solo cajeras activas (opcional)
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtrar por número de empleado (opcional)
    /// </summary>
    public int? EmployeeNumber { get; init; }

    /// <summary>
    /// Verifica si hay filtro de sucursal
    /// </summary>
    public bool HasBranchFilter() => BranchId.HasValue && BranchId.Value > 0;

    /// <summary>
    /// Verifica si hay filtro de estado activo
    /// </summary>
    public bool HasActiveFilter() => IsActive.HasValue;

    /// <summary>
    /// Verifica si hay filtro de número de empleado
    /// </summary>
    public bool HasEmployeeNumberFilter() => EmployeeNumber.HasValue && EmployeeNumber.Value > 0;

    /// <summary>
    /// Verifica si hay algún filtro aplicado
    /// </summary>
    public bool HasFilters() => 
        HasSearchTerm() || 
        HasBranchFilter() || 
        HasActiveFilter() || 
        HasEmployeeNumberFilter();

    /// <summary>
    /// Valida los parámetros de búsqueda
    /// </summary>
    public new void Validate()
    {
        base.Validate();

        if (BranchId.HasValue && BranchId.Value <= 0)
            throw new ArgumentException("El ID de sucursal debe ser mayor a 0", nameof(BranchId));

        if (EmployeeNumber.HasValue && EmployeeNumber.Value <= 0)
            throw new ArgumentException("El número de empleado debe ser mayor a 0", nameof(EmployeeNumber));
    }
}
