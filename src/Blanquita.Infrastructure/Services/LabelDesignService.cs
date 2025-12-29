using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para gestionar configuraciones de diseño de etiquetas.
/// </summary>
public class LabelDesignService : ILabelDesignService
{
    private readonly BlanquitaDbContext _context;
    private readonly ILogger<LabelDesignService> _logger;

    public LabelDesignService(BlanquitaDbContext context, ILogger<LabelDesignService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<LabelDesignDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las configuraciones de diseño de etiquetas");
            
            var designs = await _context.LabelDesigns
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.IsDefault)
                .ThenBy(d => d.Name)
                .ToListAsync(cancellationToken);

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
            
            var design = await _context.LabelDesigns
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

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
            
            var design = await _context.LabelDesigns
                .FirstOrDefaultAsync(d => d.IsDefault && d.IsActive, cancellationToken);

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

            // Si se marca como predeterminada, desmarcar las demás
            if (dto.IsDefault)
            {
                await RemoveAllDefaultsAsync(cancellationToken);
            }

            _context.LabelDesigns.Add(design);
            await _context.SaveChangesAsync(cancellationToken);

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

            var design = await _context.LabelDesigns
                .FirstOrDefaultAsync(d => d.Id == dto.Id, cancellationToken);

            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {dto.Id}");
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

            // Si se marca como predeterminada, desmarcar las demás
            if (dto.IsDefault && !design.IsDefault)
            {
                await RemoveAllDefaultsAsync(cancellationToken);
                design.SetAsDefault();
            }
            else if (!dto.IsDefault && design.IsDefault)
            {
                design.RemoveDefault();
            }

            // Actualizar estado activo
            if (dto.IsActive && !design.IsActive)
            {
                design.Activate();
            }
            else if (!dto.IsActive && design.IsActive)
            {
                design.Deactivate();
            }

            await _context.SaveChangesAsync(cancellationToken);

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

            var design = await _context.LabelDesigns
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {id}");
            }

            if (design.IsDefault)
            {
                throw new InvalidOperationException("No se puede eliminar la configuración predeterminada. Primero establezca otra como predeterminada.");
            }

            _context.LabelDesigns.Remove(design);
            await _context.SaveChangesAsync(cancellationToken);

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

            var design = await _context.LabelDesigns
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

            if (design == null)
            {
                throw new InvalidOperationException($"No se encontró la configuración de diseño con ID: {id}");
            }

            // Remover predeterminado de todas las demás
            await RemoveAllDefaultsAsync(cancellationToken);

            // Establecer como predeterminada
            design.SetAsDefault();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Configuración de diseño establecida como predeterminada: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer la configuración de diseño como predeterminada: {Id}", id);
            throw;
        }
    }

    private async Task RemoveAllDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var defaultDesigns = await _context.LabelDesigns
            .Where(d => d.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var design in defaultDesigns)
        {
            design.RemoveDefault();
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
            IsActive = design.IsActive
        };
    }
}
