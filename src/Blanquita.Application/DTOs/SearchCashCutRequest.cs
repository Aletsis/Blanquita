using Blanquita.Domain.ValueObjects;

namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO para búsqueda de cortes de caja con filtros de fecha y sucursal.
/// </summary>
public sealed class SearchCashCutRequest : DateRangeSearchRequest
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
    /// Filtrar por cajera (opcional)
    /// </summary>
    public string? CashierName { get; init; }

    /// <summary>
    /// Filtrar por supervisor (opcional)
    /// </summary>
    public string? SupervisorName { get; init; }

    /// <summary>
    /// Monto mínimo del corte (opcional)
    /// </summary>
    public decimal? MinAmount { get; init; }

    /// <summary>
    /// Monto máximo del corte (opcional)
    /// </summary>
    public decimal? MaxAmount { get; init; }

    /// <summary>
    /// Número de página para paginación (opcional)
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Tamaño de página para paginación (opcional)
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Columna por la cual ordenar (opcional)
    /// </summary>
    public string? SortColumn { get; init; }

    /// <summary>
    /// Indica si el ordenamiento es ascendente
    /// </summary>
    public bool SortAscending { get; init; } = true;

    /// <summary>
    /// Verifica si hay filtro de sucursal
    /// </summary>
    public bool HasSucursalFilter() => Sucursal != null;

    /// <summary>
    /// Verifica si hay filtro de caja registradora
    /// </summary>
    public bool HasCashRegisterFilter() => !string.IsNullOrWhiteSpace(CashRegisterName);

    /// <summary>
    /// Verifica si hay filtro de cajera
    /// </summary>
    public bool HasCashierFilter() => !string.IsNullOrWhiteSpace(CashierName);

    /// <summary>
    /// Verifica si hay filtro de supervisor
    /// </summary>
    public bool HasSupervisorFilter() => !string.IsNullOrWhiteSpace(SupervisorName);

    /// <summary>
    /// Verifica si hay filtro de monto
    /// </summary>
    public bool HasAmountFilter() => MinAmount.HasValue || MaxAmount.HasValue;

    /// <summary>
    /// Verifica si se requiere paginación
    /// </summary>
    public bool RequiresPagination() => Page.HasValue && PageSize.HasValue;

    /// <summary>
    /// Verifica si hay ordenamiento especificado
    /// </summary>
    public bool HasSorting() => !string.IsNullOrWhiteSpace(SortColumn);

    /// <summary>
    /// Verifica si hay algún filtro aplicado
    /// </summary>
    public bool HasFilters() => 
        HasDateFilter() || 
        HasSucursalFilter() || 
        HasCashRegisterFilter() || 
        HasCashierFilter() || 
        HasSupervisorFilter() || 
        HasAmountFilter();

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

        if (MinAmount.HasValue && MinAmount.Value < 0)
            throw new ArgumentException("El monto mínimo no puede ser negativo", nameof(MinAmount));

        if (MaxAmount.HasValue && MaxAmount.Value < 0)
            throw new ArgumentException("El monto máximo no puede ser negativo", nameof(MaxAmount));

        if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount.Value > MaxAmount.Value)
            throw new ArgumentException("El monto mínimo no puede ser mayor que el monto máximo");
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

    /// <summary>
    /// Obtiene el rango de montos normalizado
    /// </summary>
    public (decimal Min, decimal Max) GetAmountRange()
    {
        var min = MinAmount ?? 0;
        var max = MaxAmount ?? decimal.MaxValue;
        return (min, max);
    }
}
