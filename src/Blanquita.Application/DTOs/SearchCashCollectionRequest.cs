using Blanquita.Domain.ValueObjects;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para búsqueda de recolecciones de efectivo con filtros de fecha y sucursal.
/// </summary>
public sealed class SearchCashCollectionRequest : DateRangeSearchRequest
{
    /// <summary>
    /// Filtrar por sucursal (opcional)
    /// </summary>
    public Sucursal? Sucursal { get; init; }

    /// <summary>
    /// Filtrar por nombre de caja registradora (opcional)
    /// </summary>
    public string? CashRegisterName { get; init; }

    /// <summary>
    /// Filtrar solo recolecciones que ya fueron cortadas (opcional)
    /// </summary>
    public bool? IsCut { get; init; }

    /// <summary>
    /// Número de página para paginación (opcional)
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Tamaño de página para paginación (opcional)
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Verifica si hay filtro de sucursal
    /// </summary>
    public bool HasSucursalFilter() => Sucursal != null;

    /// <summary>
    /// Verifica si hay filtro de caja registradora
    /// </summary>
    public bool HasCashRegisterFilter() => !string.IsNullOrWhiteSpace(CashRegisterName);

    /// <summary>
    /// Verifica si hay filtro de estado de corte
    /// </summary>
    public bool HasCutStatusFilter() => IsCut.HasValue;

    /// <summary>
    /// Verifica si se requiere paginación
    /// </summary>
    public bool RequiresPagination() => Page.HasValue && PageSize.HasValue;

    /// <summary>
    /// Verifica si hay algún filtro aplicado
    /// </summary>
    public bool HasFilters() => 
        HasDateFilter() || 
        HasSucursalFilter() || 
        HasCashRegisterFilter() || 
        HasCutStatusFilter();

    /// <summary>
    /// Valida los parámetros de búsqueda
    /// </summary>
    public new void Validate()
    {
        base.Validate();

        if (Page.HasValue && Page.Value < 1)
            throw new ArgumentException("El número de página debe ser mayor o igual a 1", nameof(Page));

        if (PageSize.HasValue && PageSize.Value < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1", nameof(PageSize));

        if (PageSize.HasValue && PageSize.Value > 100)
            throw new ArgumentException("El tamaño de página no puede ser mayor a 100", nameof(PageSize));
    }

    /// <summary>
    /// Calcula el número de elementos a saltar para la paginación
    /// </summary>
    public int GetSkip()
    {
        if (!RequiresPagination())
            return 0;

        return (Page!.Value - 1) * PageSize!.Value;
    }
}
