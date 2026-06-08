using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blanquita.Application.Services;

/// <summary>
/// Implementación del servicio de configuración del sistema (Capa de Aplicación)
/// </summary>
public class ConfiguracionService : IConfiguracionService
{
    private readonly ISystemConfigurationRepository _repository;
    private readonly IFileSystemService _fileSystemService;
    private readonly IAppConfigurationManager _legacyConfigManager; // For migration
    private readonly ILogger<ConfiguracionService> _logger;

    private static DTOs.ConfiguracionDto? _staticCachedConfig;
    private static readonly System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1, 1);

    public static void ClearCache()
    {
        _semaphore.Wait();
        try
        {
            _staticCachedConfig = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public ConfiguracionService(
        ISystemConfigurationRepository repository,
        IFileSystemService fileSystemService,
        IAppConfigurationManager legacyConfigManager,
        ILogger<ConfiguracionService> logger)
    {
        _repository = repository;
        _fileSystemService = fileSystemService;
        _legacyConfigManager = legacyConfigManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ConfiguracionDto> ObtenerConfiguracionAsync()
    {
        if (_staticCachedConfig != null)
        {
            return _staticCachedConfig;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_staticCachedConfig != null)
            {
                return _staticCachedConfig;
            }

            _logger.LogInformation("Obteniendo configuración del sistema desde la base de datos");
            
            var configEntity = await _repository.GetAsync();
            
            if (configEntity == null)
            {
                configEntity = new SystemConfiguration();
                
                // Intentar migrar configuración existente desde JSON
                try 
                {
                    var oldConfig = _legacyConfigManager.CargarConfiguracion();
                    if (oldConfig != null)
                    {
                         configEntity.Pos10041Path = oldConfig.Pos10041Path ?? string.Empty;
                         configEntity.Pos10042Path = oldConfig.Pos10042Path ?? string.Empty;
                         configEntity.Mgw10008Path = oldConfig.Mgw10008Path ?? string.Empty;
                         configEntity.Mgw10005Path = oldConfig.Mgw10005Path ?? string.Empty;
                         configEntity.PrinterName = oldConfig.PrinterName ?? string.Empty;
                         configEntity.PrinterIp = oldConfig.PrinterIp ?? string.Empty;
                         
                         if (int.TryParse(oldConfig.PrinterPort, out var p1)) configEntity.PrinterPort = p1;
                         
                         configEntity.Printer2Name = oldConfig.Printer2Name ?? string.Empty;
                         configEntity.Printer2Ip = oldConfig.Printer2Ip ?? string.Empty;
                         
                         if (int.TryParse(oldConfig.Printer2Port, out var p2)) configEntity.Printer2Port = p2;
                         
                         _logger.LogInformation("Configuración antigua migrada exitosamente");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo migrar la configuración antigua. Se usará una nueva.");
                }

                await _repository.AddAsync(configEntity);
            }

            _staticCachedConfig = MapearADto(configEntity);
            return _staticCachedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la configuración del sistema");
            throw;
        }
        finally
        {
            _semaphore.Release();
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

            await _semaphore.WaitAsync();
            try
            {
                var configEntity = await _repository.GetAsync();
                if (configEntity == null)
                {
                    configEntity = new SystemConfiguration();
                    ActualizarEntidad(configEntity, configuracion);
                    await _repository.AddAsync(configEntity);
                }
                else
                {
                    ActualizarEntidad(configEntity, configuracion);
                    await _repository.UpdateAsync(configEntity);
                }

                // Invalidad caché
                _staticCachedConfig = null;
            }
            finally
            {
                _semaphore.Release();
            }

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
            ValidarRutaDbf(configuracion.Mgw10002Path, "MGW10002.DBF", resultado);
            ValidarRutaDbf(configuracion.Mgw10011Path, "MGW10011.DBF", resultado);
            ValidarRutaDbf(configuracion.Pos10008Path, "POS10008.DBF", resultado);
            ValidarRutaDbf(configuracion.Pos10010Path, "POS10010.DBF", resultado);

            // Validar ruta de facturas
            if (!string.IsNullOrWhiteSpace(configuracion.FacturasPath))
            {
                if (!_fileSystemService.DirectoryExists(configuracion.FacturasPath))
                {
                    resultado.AgregarError("La ruta de facturas no existe");
                }
            }

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
        return _fileSystemService.FileExists(ruta);
    }

    /// <inheritdoc/>
    public bool ValidarRutaDirectorio(string ruta)
    {
        return _fileSystemService.DirectoryExists(ruta);
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
            TipoArchivoDbf.Mgw10002 => "MGW10002.DBF",
            TipoArchivoDbf.Mgw10011 => "MGW10011.DBF",
            TipoArchivoDbf.Pos10008 => "POS10008.DBF",
            TipoArchivoDbf.Pos10010 => "POS10010.DBF",
            _ => "archivo.dbf"
        };
    }

    /// <inheritdoc/>
    public async Task RestablecerConfiguracionAsync()
    {
        try
        {
            _logger.LogInformation("Restableciendo configuración a valores predeterminados");
            
            await _semaphore.WaitAsync();
            try
            {
                var configEntity = await _repository.GetAsync();
                if (configEntity == null)
                {
                    configEntity = new SystemConfiguration();
                    await _repository.AddAsync(configEntity);
                }
                else
                {
                    ActualizarEntidad(configEntity, new ConfiguracionDto());
                    await _repository.UpdateAsync(configEntity);
                }

                _staticCachedConfig = null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer la configuración");
            throw;
        }
    }

    private void ValidarRutaDbf(string ruta, string nombreArchivo, ResultadoValidacionConfiguracion resultado)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            resultado.AgregarError($"La ruta de {nombreArchivo} es obligatoria");
        }
        else if (!_fileSystemService.FileExists(ruta))
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
            Mgw10002Path = entity.Mgw10002Path,
            Mgw10011Path = entity.Mgw10011Path,
            Pos10008Path = entity.Pos10008Path,
            Pos10010Path = entity.Pos10010Path,
            FacturasPath = entity.FacturasPath,
            PrinterName = entity.PrinterName,
            PrinterIp = entity.PrinterIp,
            PrinterPort = entity.PrinterPort,
            Printer2Name = entity.Printer2Name,
            Printer2Ip = entity.Printer2Ip,
            Printer2Port = entity.Printer2Port,
            SmtpServer = entity.SmtpServer,
            SmtpPort = entity.SmtpPort,
            SmtpUser = entity.SmtpUser,
            SmtpPassword = entity.SmtpPassword,
            SmtpEnableSsl = entity.SmtpEnableSsl,
            SmtpFromEmail = entity.SmtpFromEmail,
            SmtpFromName = entity.SmtpFromName,
            InvoiceJobExecutionTime = entity.InvoiceJobExecutionTime,
            AlertEmails = entity.AlertEmails,
            CommercialApiUrl = entity.CommercialApiUrl,
            CommercialApiKey = entity.CommercialApiKey,
            WhatsAppServiceUrl = entity.WhatsAppServiceUrl
        };
    }
 
    private void ActualizarEntidad(SystemConfiguration entity, ConfiguracionDto dto)
    {
        entity.Pos10041Path = dto.Pos10041Path;
        entity.Pos10042Path = dto.Pos10042Path;
        entity.Mgw10008Path = dto.Mgw10008Path;
        entity.Mgw10005Path = dto.Mgw10005Path;
        entity.Mgw10045Path = dto.Mgw10045Path;
        entity.Mgw10002Path = dto.Mgw10002Path;
        entity.Mgw10011Path = dto.Mgw10011Path;
        entity.Pos10008Path = dto.Pos10008Path;
        entity.Pos10010Path = dto.Pos10010Path;
        entity.FacturasPath = dto.FacturasPath;
        entity.PrinterName = dto.PrinterName;
        entity.PrinterIp = dto.PrinterIp;
        entity.PrinterPort = dto.PrinterPort;
        entity.Printer2Name = dto.Printer2Name;
        entity.Printer2Ip = dto.Printer2Ip;
        entity.Printer2Port = dto.Printer2Port;
        entity.SmtpServer = dto.SmtpServer;
        entity.SmtpPort = dto.SmtpPort;
        entity.SmtpUser = dto.SmtpUser;
        entity.SmtpPassword = dto.SmtpPassword;
        entity.SmtpEnableSsl = dto.SmtpEnableSsl;
        entity.SmtpFromEmail = dto.SmtpFromEmail;
        entity.SmtpFromName = dto.SmtpFromName;
        entity.InvoiceJobExecutionTime = dto.InvoiceJobExecutionTime;
        entity.AlertEmails = dto.AlertEmails;
        entity.CommercialApiUrl = dto.CommercialApiUrl;
        entity.CommercialApiKey = dto.CommercialApiKey;
        entity.WhatsAppServiceUrl = dto.WhatsAppServiceUrl;
    }
}
