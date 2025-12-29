using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para gestionar las configuraciones de diseño de etiquetas.
/// </summary>
public interface ILabelDesignService
{
    /// <summary>
    /// Obtiene todas las configuraciones de diseño de etiquetas.
    /// </summary>
    Task<List<LabelDesignDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una configuración de diseño por su ID.
    /// </summary>
    Task<LabelDesignDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la configuración de diseño predeterminada.
    /// </summary>
    Task<LabelDesignDto?> GetDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva configuración de diseño de etiqueta.
    /// </summary>
    Task<LabelDesignDto> CreateAsync(LabelDesignDto design, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una configuración de diseño existente.
    /// </summary>
    Task<LabelDesignDto> UpdateAsync(LabelDesignDto design, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una configuración de diseño.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establece una configuración como predeterminada.
    /// </summary>
    Task SetAsDefaultAsync(int id, CancellationToken cancellationToken = default);
}
