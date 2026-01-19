using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Infrastructure.Models;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de configuración del sistema
/// </summary>
public class ConfiguracionService : IConfiguracionService
{
    private readonly BlanquitaDbContext _dbContext;
    private readonly IAppConfigurationManager _configurationManager;
    private readonly ILogger<ConfiguracionService> _logger;

    public ConfiguracionService(
        BlanquitaDbContext dbContext,
        IAppConfigurationManager configurationManager,
        ILogger<ConfiguracionService> logger)
    {
        _dbContext = dbContext;
        _configurationManager = configurationManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ConfiguracionDto> ObtenerConfiguracionAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo configuración del sistema");
            
            var configEntity = await _dbContext.SystemConfigurations.FirstOrDefaultAsync();
            
            if (configEntity == null)
            {
                configEntity = new SystemConfiguration();
                
                // Intentar migrar configuración existente desde JSON
                try 
                {
                    var oldConfig = _configurationManager.CargarConfiguracion();
                    if (oldConfig != null)
                    {
                         configEntity.Pos10041Path = oldConfig.Pos10041Path ?? string.Empty;
                         configEntity.Pos10042Path = oldConfig.Pos10042Path ?? string.Empty;
                         configEntity.Mgw10008Path = oldConfig.Mgw10008Path ?? string.Empty;
                         configEntity.Mgw10005Path = oldConfig.Mgw10005Path ?? string.Empty;
                         configEntity.Mgw10045Path = string.Empty; // Valor por defecto
                         configEntity.PrinterName = oldConfig.PrinterName ?? string.Empty;
                         configEntity.PrinterIp = oldConfig.PrinterIp ?? string.Empty;
                         
                         if (int.TryParse(oldConfig.PrinterPort, out var p1)) configEntity.PrinterPort = p1;
                         
                         configEntity.Printer2Name = oldConfig.Printer2Name ?? string.Empty;
                         configEntity.Printer2Ip = oldConfig.Printer2Ip ?? string.Empty;
                         
                         if (int.TryParse(oldConfig.Printer2Port, out var p2)) configEntity.Printer2Port = p2;
                         
                         _logger.LogInformation("Configuración antigua migrada exitosamente a la base de datos");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo migrar la configuración antigua desde JSON. Se usará una nueva.");
                }

                _dbContext.SystemConfigurations.Add(configEntity);
                await _dbContext.SaveChangesAsync();
            }

            var dto = MapearADto(configEntity);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la configuración del sistema");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task GuardarConfiguracionAsync(ConfiguracionDto configuracion)
    {
        try
        {
            _logger.LogInformation("Guardando configuración del sistema");

            // Validar antes de guardar
            var validacion = await ValidarConfiguracionAsync(configuracion);
            if (!validacion.EsValido)
            {
                var erroresTexto = string.Join(", ", validacion.Errores);
                throw new InvalidOperationException($"La configuración no es válida: {erroresTexto}");
            }

            var configEntity = await _dbContext.SystemConfigurations.FirstOrDefaultAsync();
            if (configEntity == null)
            {
                configEntity = new SystemConfiguration();
                _dbContext.SystemConfigurations.Add(configEntity);
            }

            ActualizarEntidad(configEntity, configuracion);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Configuración guardada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar la configuración del sistema");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<ResultadoValidacionConfiguracion> ValidarConfiguracionAsync(ConfiguracionDto configuracion)
    {
        var resultado = new ResultadoValidacionConfiguracion();

        try
        {
            // Validar rutas de archivos DBF
            ValidarRutaDbf(configuracion.Pos10041Path, "POS10041.DBF", resultado);
            ValidarRutaDbf(configuracion.Pos10042Path, "POS10042.DBF", resultado);
            ValidarRutaDbf(configuracion.Mgw10008Path, "MGW10008.DBF", resultado);
            ValidarRutaDbf(configuracion.Mgw10005Path, "MGW10005.DBF", resultado);
            ValidarRutaDbf(configuracion.Mgw10045Path, "MGW10045.DBF", resultado);

            // Validar configuración de impresoras (advertencias, no errores)
            if (string.IsNullOrWhiteSpace(configuracion.PrinterName))
            {
                resultado.AgregarAdvertencia("No se ha configurado el nombre de la impresora principal");
            }

            if (string.IsNullOrWhiteSpace(configuracion.PrinterIp))
            {
                resultado.AgregarAdvertencia("No se ha configurado la IP de la impresora principal");
            }

            if (configuracion.PrinterPort <= 0)
            {
                resultado.AgregarAdvertencia("El puerto de la impresora principal no es válido");
            }

            // Si no hay errores, marcar como válido
            if (resultado.Errores.Count == 0)
            {
                resultado.EsValido = true;
            }

            return Task.FromResult(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la validación de configuración");
            resultado.AgregarError($"Error durante la validación: {ex.Message}");
            return Task.FromResult(resultado);
        }
    }

    /// <inheritdoc/>
    public bool ValidarRutaArchivo(string ruta)
    {
        return _configurationManager.ValidatePath(ruta);
    }

    /// <inheritdoc/>
    public string ObtenerNombreArchivo(TipoArchivoDbf tipo)
    {
        return tipo switch
        {
            TipoArchivoDbf.Pos10041 => "POS10041.DBF",
            TipoArchivoDbf.Pos10042 => "POS10042.DBF",
            TipoArchivoDbf.Mgw10008 => "MGW10008.DBF",
            TipoArchivoDbf.Mgw10005 => "MGW10005.DBF",
            TipoArchivoDbf.Mgw10045 => "MGW10045.DBF",
            _ => "archivo.dbf"
        };
    }

    /// <inheritdoc/>
    public async Task RestablecerConfiguracionAsync()
    {
        try
        {
            _logger.LogInformation("Restableciendo configuración a valores predeterminados");
            var configuracionPredeterminada = new ConfiguracionDto();
            await GuardarConfiguracionAsync(configuracionPredeterminada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer la configuración");
            throw;
        }
    }

    #region Métodos privados de validación y mapeo

    private void ValidarRutaDbf(string ruta, string nombreArchivo, ResultadoValidacionConfiguracion resultado)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            resultado.AgregarError($"La ruta de {nombreArchivo} es obligatoria");
        }
        else if (!ValidarRutaArchivo(ruta))
        {
            resultado.AgregarError($"El archivo {nombreArchivo} no existe en la ruta especificada");
        }
    }

    private ConfiguracionDto MapearADto(SystemConfiguration entity)
    {
        return new ConfiguracionDto
        {
            Pos10041Path = entity.Pos10041Path,
            Pos10042Path = entity.Pos10042Path,
            Mgw10008Path = entity.Mgw10008Path,
            Mgw10005Path = entity.Mgw10005Path,
            Mgw10045Path = entity.Mgw10045Path,
            PrinterName = entity.PrinterName,
            PrinterIp = entity.PrinterIp,
            PrinterPort = entity.PrinterPort,
            Printer2Name = entity.Printer2Name,
            Printer2Ip = entity.Printer2Ip,
            Printer2Port = entity.Printer2Port
        };
    }

    private void ActualizarEntidad(SystemConfiguration entity, ConfiguracionDto dto)
    {
        entity.Pos10041Path = dto.Pos10041Path;
        entity.Pos10042Path = dto.Pos10042Path;
        entity.Mgw10008Path = dto.Mgw10008Path;
        entity.Mgw10005Path = dto.Mgw10005Path;
        entity.Mgw10045Path = dto.Mgw10045Path;
        entity.PrinterName = dto.PrinterName;
        entity.PrinterIp = dto.PrinterIp;
        entity.PrinterPort = dto.PrinterPort;
        entity.Printer2Name = dto.Printer2Name;
        entity.Printer2Ip = dto.Printer2Ip;
        entity.Printer2Port = dto.Printer2Port;
    }

    #endregion
}
