using Blanquita.Domain.ValueObjects;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para búsqueda paginada de cajas registradoras con filtros específicos.
/// </summary>
public sealed class SearchCashRegisterRequest : PagedSearchRequest
{
    /// <summary>
    /// Filtrar por sucursal (opcional)
    /// </summary>
    public Sucursal? Sucursal { get; init; }

    /// <summary>
    /// Filtrar solo cajas activas (opcional)
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtrar por nombre de caja (opcional)
    /// </summary>
    public string? CashRegisterName { get; init; }

    /// <summary>
    /// Verifica si hay filtro de sucursal
    /// </summary>
    public bool HasSucursalFilter() => Sucursal != null;

    /// <summary>
    /// Verifica si hay filtro de estado activo
    /// </summary>
    public bool HasActiveFilter() => IsActive.HasValue;

    /// <summary>
    /// Verifica si hay filtro de nombre de caja
    /// </summary>
    public bool HasCashRegisterNameFilter() => !string.IsNullOrWhiteSpace(CashRegisterName);

    /// <summary>
    /// Verifica si hay algún filtro aplicado
    /// </summary>
    public bool HasFilters() => 
        HasSearchTerm() || 
        HasSucursalFilter() || 
        HasActiveFilter() || 
        HasCashRegisterNameFilter();

    /// <summary>
    /// Valida los parámetros de búsqueda
    /// </summary>
    public new void Validate()
    {
        base.Validate();
    }
}
