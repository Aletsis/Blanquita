namespace Blanquita.Application.DTOs;

/// <summary>
/// DTO base para solicitudes de búsqueda paginada.
/// Encapsula parámetros comunes de paginación y ordenamiento.
/// </summary>
public class PagedSearchRequest
{
    /// <summary>
    /// Número de página (basado en 1)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Tamaño de página (número de elementos por página)
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Columna por la cual ordenar (opcional)
    /// </summary>
    public string? SortColumn { get; init; }

    /// <summary>
    /// Indica si el ordenamiento es ascendente (true) o descendente (false)
    /// </summary>
    public bool SortAscending { get; init; } = true;

    /// <summary>
    /// Término de búsqueda general (opcional)
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Valida que los parámetros de paginación sean correctos
    /// </summary>
    public void Validate()
    {
        if (Page < 1)
            throw new ArgumentException("El número de página debe ser mayor o igual a 1", nameof(Page));

        if (PageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1", nameof(PageSize));

        if (PageSize > 100)
            throw new ArgumentException("El tamaño de página no puede ser mayor a 100", nameof(PageSize));
    }

    /// <summary>
    /// Calcula el número de elementos a saltar para la paginación
    /// </summary>
    public int GetSkip() => (Page - 1) * PageSize;

    /// <summary>
    /// Verifica si hay un término de búsqueda
    /// </summary>
    public bool HasSearchTerm() => !string.IsNullOrWhiteSpace(SearchTerm);

    /// <summary>
    /// Verifica si hay ordenamiento especificado
    /// </summary>
    public bool HasSorting() => !string.IsNullOrWhiteSpace(SortColumn);
}
