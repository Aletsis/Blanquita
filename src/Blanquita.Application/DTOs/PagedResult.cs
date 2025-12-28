namespace Blanquita.Application.DTOs;

/// <summary>
/// Resultado paginado genérico que encapsula una colección de elementos
/// junto con información de paginación.
/// </summary>
/// <typeparam name="T">Tipo de elementos en la colección</typeparam>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    /// <summary>
    /// Calcula el número total de páginas
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Indica si hay una página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica si hay una página siguiente
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indica si es la primera página
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;

    /// <summary>
    /// Indica si es la última página
    /// </summary>
    public bool IsLastPage => PageNumber == TotalPages;

    /// <summary>
    /// Obtiene el número del primer elemento en la página actual (basado en 1)
    /// </summary>
    public int FirstItemNumber => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

    /// <summary>
    /// Obtiene el número del último elemento en la página actual (basado en 1)
    /// </summary>
    public int LastItemNumber => Math.Min(PageNumber * PageSize, TotalCount);

    /// <summary>
    /// Indica si hay elementos en el resultado
    /// </summary>
    public bool HasItems => Items.Any();

    /// <summary>
    /// Indica si el resultado está vacío
    /// </summary>
    public bool IsEmpty => !HasItems;

    /// <summary>
    /// Crea un resultado paginado vacío
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResult<T>
        {
            Items = Enumerable.Empty<T>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Crea un resultado paginado a partir de una colección
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Mapea los elementos del resultado a otro tipo
    /// </summary>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new PagedResult<TResult>
        {
            Items = Items.Select(mapper),
            TotalCount = TotalCount,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
