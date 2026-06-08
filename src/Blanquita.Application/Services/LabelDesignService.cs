using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

/// <summary>
/// Implementación del servicio para gestionar configuraciones de diseño de etiquetas (Capa de Aplicación).
/// </summary>
public class LabelDesignService : ILabelDesignService
{
    private readonly ILabelDesignRepository _repository;
    private readonly ILogger<LabelDesignService> _logger;

    public LabelDesignService(ILabelDesignRepository repository, ILogger<LabelDesignService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<LabelDesignDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las configuraciones de diseño de etiquetas");
            var designs = await _repository.GetAllAsync(cancellationToken);
            return designs.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las configuraciones de diseño de etiquetas");
            throw;
        }
    }

    public async Task<LabelDesignDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo configuración de diseño con ID: {Id}", id);
            var design = await _repository.GetByIdAsync(id, cancellationToken);
            return design == null ? null : MapToDto(design);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la configuración de diseño con ID: {Id}", id);
            throw;
        }
    }

    public async Task<LabelDesignDto?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo configuración de diseño predeterminada");
            var design = await _repository.GetDefaultAsync(cancellationToken);
            return design == null ? null : MapToDto(design);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la configuración de diseño predeterminada");
            throw;
        }
    }

    public async Task<LabelDesignDto> CreateAsync(LabelDesignDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creando nueva configuración de diseño: {Name}", dto.Name);

            if (await _repository.ExistsAsync(dto.Name, cancellationToken))
            {
                throw new InvalidOperationException($"Ya existe una configuración de diseño con el nombre '{dto.Name}'.");
            }

            var design = LabelDesign.Create(
                dto.Name,
                dto.WidthInMm,
                dto.HeightInMm,
                dto.MarginTopInMm,
                dto.MarginLeftInMm,
                dto.Orientation,
                dto.ProductNameFontSize,
                dto.ProductCodeFontSize,
                dto.PriceFontSize,
                dto.BarcodeHeightInMm,
                dto.BarcodeWidth,
                dto.IsDefault
            );

            if (dto.Elements != null)
            {
                foreach (var el in dto.Elements)
                {
                    design.AddElement(LabelElement.Create(
                        el.ElementType,
                        el.XMm,
                        el.YMm,
                        el.Content,
                        el.FontSize,
                        el.HeightMm,
                        el.BarWidth
                    ));
                }
            }

            if (dto.IsDefault)
            {
                await _repository.RemoveAllDefaultsAsync(cancellationToken);
            }

            await _repository.AddAsync(design, cancellationToken);

            _logger.LogInformation("Configuración de diseño creada exitosamente con ID: {Id}", design.Id);
            return MapToDto(design);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la configuración de diseño: {Name}", dto.Name);
            throw;
        }
    }

    public async Task<LabelDesignDto> UpdateAsync(LabelDesignDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando configuración de diseño con ID: {Id}", dto.Id);

            var design = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {dto.Id}");
            }

            if (design.Name != dto.Name && await _repository.ExistsAsync(dto.Name, cancellationToken))
            {
                throw new InvalidOperationException($"Ya existe una configuración de diseño con el nombre '{dto.Name}'.");
            }

            design.Update(
                dto.Name,
                dto.WidthInMm,
                dto.HeightInMm,
                dto.MarginTopInMm,
                dto.MarginLeftInMm,
                dto.Orientation,
                dto.ProductNameFontSize,
                dto.ProductCodeFontSize,
                dto.PriceFontSize,
                dto.BarcodeHeightInMm,
                dto.BarcodeWidth
            );

            design.ClearElements();
            if (dto.Elements != null)
            {
                foreach (var el in dto.Elements)
                {
                    design.AddElement(LabelElement.Create(
                        el.ElementType,
                        el.XMm,
                        el.YMm,
                        el.Content,
                        el.FontSize,
                        el.HeightMm,
                        el.BarWidth
                    ));
                }
            }

            if (dto.IsDefault && !design.IsDefault)
            {
                await _repository.RemoveAllDefaultsAsync(cancellationToken);
                design.SetAsDefault();
            }
            else if (!dto.IsDefault && design.IsDefault)
            {
                design.RemoveDefault();
            }

            if (dto.IsActive && !design.IsActive)
            {
                design.Activate();
            }
            else if (!dto.IsActive && design.IsActive)
            {
                design.Deactivate();
            }

            await _repository.UpdateAsync(design, cancellationToken);

            _logger.LogInformation("Configuración de diseño actualizada exitosamente: {Id}", dto.Id);
            return MapToDto(design);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la configuración de diseño con ID: {Id}", dto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Eliminando configuración de diseño con ID: {Id}", id);

            var design = await _repository.GetByIdAsync(id, cancellationToken);
            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {id}");
            }

            if (design.IsDefault)
            {
                throw new InvalidOperationException("No se puede eliminar la configuración predeterminada. Primero establezca otra como predeterminada.");
            }

            await _repository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Configuración de diseño eliminada exitosamente: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la configuración de diseño con ID: {Id}", id);
            throw;
        }
    }

    public async Task SetAsDefaultAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Estableciendo configuración de diseño como predeterminada: {Id}", id);

            var design = await _repository.GetByIdAsync(id, cancellationToken);
            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {id}");
            }

            await _repository.RemoveAllDefaultsAsync(cancellationToken);
            design.SetAsDefault();
            await _repository.UpdateAsync(design, cancellationToken);

            _logger.LogInformation("Configuración de diseño establecida como predeterminada: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer la configuración de diseño como predeterminada: {Id}", id);
            throw;
        }
    }

    private static LabelDesignDto MapToDto(LabelDesign design)
    {
        return new LabelDesignDto
        {
            Id = design.Id,
            Name = design.Name,
            WidthInMm = design.WidthInMm,
            HeightInMm = design.HeightInMm,
            MarginTopInMm = design.MarginTopInMm,
            MarginLeftInMm = design.MarginLeftInMm,
            Orientation = design.Orientation,
            ProductNameFontSize = design.ProductNameFontSize,
            ProductCodeFontSize = design.ProductCodeFontSize,
            PriceFontSize = design.PriceFontSize,
            BarcodeHeightInMm = design.BarcodeHeightInMm,
            BarcodeWidth = design.BarcodeWidth,
            IsDefault = design.IsDefault,
            IsActive = design.IsActive,
            Elements = design.Elements.Select(e => new LabelElementDto
            {
                 Id = e.Id,
                 LabelDesignId = e.LabelDesignId,
                 ElementType = e.ElementType,
                 XMm = e.XMm,
                 YMm = e.YMm,
                 Content = e.Content,
                 FontSize = e.FontSize,
                 HeightMm = e.HeightMm,
                 BarWidth = e.BarWidth
            }).ToList()
        };
    }
}
