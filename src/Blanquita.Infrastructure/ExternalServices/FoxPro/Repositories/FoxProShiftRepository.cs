using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

public class FoxProShiftRepository : IFoxProShiftRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProShiftRepository> _logger;

    public FoxProShiftRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProShiftRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<ShiftConciliationDataDto?> GetShiftDataAsync(int internalId, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10042Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);
            while (reader.Read())
            {
                var currentInternalId = reader.HasColumn("CIDPOS01") ? reader.GetInt32Safe("CIDPOS01") : reader.GetInt32Safe("CIDAPERTUR");
                if (currentInternalId == internalId)
                {
                    var shiftDate = reader.GetDateTimeSafe("CFECHAAPER");
                    if (shiftDate == DateTime.MinValue) shiftDate = reader.GetDateTimeSafe("CFECHA");
                    
                    // Intentar obtener la hora de apertura desde CHORAAPER o CHORA
                    var hora = reader.HasColumn("CHORAAPER") ? reader.GetStringSafe("CHORAAPER") : string.Empty;
                    if (string.IsNullOrEmpty(hora) && reader.HasColumn("CHORA")) hora = reader.GetStringSafe("CHORA");
                    
                    var openingTime = CombineDateAndStringTime(shiftDate, hora);
                    
                    var horaCor = reader.HasColumn("CHORACOR") ? reader.GetStringSafe("CHORACOR") : string.Empty;
                    if (string.IsNullOrEmpty(horaCor) && reader.HasColumn("CHORA")) horaCor = reader.GetStringSafe("CHORA");
                    
                    var closingTime = string.IsNullOrEmpty(horaCor) ? (DateTime?)null : CombineDateAndStringTime(shiftDate, horaCor);

                    return new ShiftConciliationDataDto
                    {
                        InternalId = currentInternalId,
                        IdContpaqi = reader.GetInt32Safe("CIDCAJA"),
                        CashierId = reader.HasColumn("CIDAGENTE") ? reader.GetInt32Safe("CIDAGENTE") : reader.GetInt32Safe("CIDUSUARIO"),
                        OpeningTime = openingTime,
                        ClosingTime = closingTime,
                        Status = reader.GetInt32Safe("CESTADOAPE"),
                        CashCollected = reader.GetDecimalSafe("CVTAEFECT"),
                        CardCollected = reader.GetDecimalSafe("CVTATARJE"),
                        ReturnsTotal = reader.GetDecimalSafe("CAPIMPORT2")
                    };
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer dato de apertura específica {Id}", internalId);
            return null;
        }
    }

    public async Task<IEnumerable<ShiftConciliationDataDto>> GetTodayShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10042Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10042 no encontrado o no configurado");
            return Enumerable.Empty<ShiftConciliationDataDto>();
        }

        try
        {
            using var reader = _readerFactory.CreateReader(filePath);
            var results = new List<ShiftConciliationDataDto>();

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var boxId = reader.GetInt32Safe("CIDCAJA");
                var shiftDate = reader.GetDateTimeSafe("CFECHAAPER");
                if (shiftDate == DateTime.MinValue) shiftDate = reader.GetDateTimeSafe("CFECHA");
                if (shiftDate == DateTime.MinValue) shiftDate = reader.GetDateTimeSafe("CFECHACOR");

                    if (shiftDate.Date == date.Date)
                    {
                        // Intentar obtener la hora de apertura desde CHORAAPER o CHORA
                        var hora = reader.HasColumn("CHORAAPER") ? reader.GetStringSafe("CHORAAPER") : string.Empty;
                        if (string.IsNullOrEmpty(hora) && reader.HasColumn("CHORA")) hora = reader.GetStringSafe("CHORA");
                        
                        var openingTime = CombineDateAndStringTime(shiftDate, hora);

                        var horaCor = reader.HasColumn("CHORACOR") ? reader.GetStringSafe("CHORACOR") : string.Empty;
                        if (string.IsNullOrEmpty(horaCor) && reader.HasColumn("CHORA")) horaCor = reader.GetStringSafe("CHORA");
                        
                        var closingTime = string.IsNullOrEmpty(horaCor) ? (DateTime?)null : CombineDateAndStringTime(shiftDate, horaCor);

                    results.Add(new ShiftConciliationDataDto
                    {
                        InternalId = reader.HasColumn("CIDPOS01") ? reader.GetInt32Safe("CIDPOS01") : reader.GetInt32Safe("CIDAPERTUR"),
                        IdContpaqi = boxId,
                        CashierId = reader.HasColumn("CIDAGENTE") ? reader.GetInt32Safe("CIDAGENTE") : reader.GetInt32Safe("CIDUSUARIO"),
                        OpeningTime = openingTime,
                        ClosingTime = closingTime,
                        Status = reader.GetInt32Safe("CESTADOAPE"),
                        CashCollected = reader.GetDecimalSafe("CVTAEFECT"),
                        CardCollected = reader.GetDecimalSafe("CVTATARJE"),
                        ReturnsTotal = reader.GetDecimalSafe("CAPIMPORT2")
                    });
                }
            }

            return results.OrderByDescending(r => r.OpeningTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer datos de aperturas desde FoxPro");
            return Enumerable.Empty<ShiftConciliationDataDto>();
        }
    }

    private DateTime CombineDateAndStringTime(DateTime date, string timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr)) return date;
        
        try 
        {
            // Formato esperado: HH:mm:ss o HH:mm
            var parts = timeStr.Trim().Split(':');
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
                {
                    int s = parts.Length > 2 && int.TryParse(parts[2].Split(' ')[0], out int sec) ? sec : 0;
                    
                    // Manejar AM/PM si está presente
                    if (timeStr.Contains("PM", StringComparison.OrdinalIgnoreCase) && h < 12) h += 12;
                    if (timeStr.Contains("AM", StringComparison.OrdinalIgnoreCase) && h == 12) h = 0;

                    return new DateTime(date.Year, date.Month, date.Day, h, m, s);
                }
            }
            
            // Si el split falló, intentar con DateTime.TryParse
            if (DateTime.TryParse(timeStr, out var parsedTime))
            {
                return new DateTime(date.Year, date.Month, date.Day, parsedTime.Hour, parsedTime.Minute, parsedTime.Second);
            }

            // Si es un número (ej. segundos del día o formato HHMMSS)
            if (long.TryParse(timeStr.Replace(":", ""), out long timeNum))
            {
                if (timeStr.Length >= 4 && timeStr.Length <= 6) // Probable HHMM o HHMMSS
                {
                    string t = timeStr.PadLeft(6, '0');
                    if (int.TryParse(t.Substring(0, 2), out int hh) && 
                        int.TryParse(t.Substring(2, 2), out int mm))
                    {
                        int ss = int.TryParse(t.Substring(4, 2), out int sss) ? sss : 0;
                        if (hh < 24 && mm < 60 && ss < 60)
                            return new DateTime(date.Year, date.Month, date.Day, hh, mm, ss);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al parsear hora '{TimeStr}' para la fecha {Date}", timeStr, date);
        }

        return date;
    }
}
