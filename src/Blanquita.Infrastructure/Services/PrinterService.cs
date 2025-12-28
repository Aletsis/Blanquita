using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

public class PrinterService : IPrinterService
{
    private readonly BlanquitaDbContext _context;
    private readonly ILogger<PrinterService> _logger;

    public PrinterService(BlanquitaDbContext context, ILogger<PrinterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PrinterDto>> GetAllAsync()
    {
        try
        {
            return await _context.Printers
                .Where(p => p.IsActive)
                .Select(p => new PrinterDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    IpAddress = p.IpAddress,
                    Port = p.Port,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all printers");
            throw;
        }
    }

    public async Task<PrinterDto?> GetByIdAsync(int id)
    {
        try
        {
            var printer = await _context.Printers.FindAsync(id);
            if (printer == null) return null;

            return new PrinterDto
            {
                Id = printer.Id,
                Name = printer.Name,
                IpAddress = printer.IpAddress,
                Port = printer.Port,
                IsActive = printer.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting printer by id {Id}", id);
            throw;
        }
    }

    public async Task<PrinterDto> CreateAsync(PrinterDto printerDto)
    {
        try
        {
            var printer = new Printer
            {
                Name = printerDto.Name,
                IpAddress = printerDto.IpAddress,
                Port = printerDto.Port,
                IsActive = true
            };

            _context.Printers.Add(printer);
            await _context.SaveChangesAsync();

            printerDto.Id = printer.Id;
            printerDto.IsActive = printer.IsActive;
            return printerDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating printer");
            throw;
        }
    }

    public async Task UpdateAsync(PrinterDto printerDto)
    {
        try
        {
            var printer = await _context.Printers.FindAsync(printerDto.Id);
            if (printer == null) throw new KeyNotFoundException($"Printer with id {printerDto.Id} not found");

            printer.Name = printerDto.Name;
            printer.IpAddress = printerDto.IpAddress;
            printer.Port = printerDto.Port;
            // printer.IsActive = printerDto.IsActive; // Typically update doesn't toggle active status unless intended

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating printer {Id}", printerDto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var printer = await _context.Printers.FindAsync(id);
            if (printer == null) throw new KeyNotFoundException($"Printer with id {id} not found");

            printer.IsActive = false;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting printer {Id}", id);
            throw;
        }
    }
}
