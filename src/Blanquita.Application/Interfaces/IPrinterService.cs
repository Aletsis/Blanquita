using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IPrinterService
{
    Task<List<PrinterDto>> GetAllAsync();
    Task<PrinterDto?> GetByIdAsync(int id);
    Task<PrinterDto> CreateAsync(PrinterDto printer);
    Task UpdateAsync(PrinterDto printer);
    Task DeleteAsync(int id);
}
