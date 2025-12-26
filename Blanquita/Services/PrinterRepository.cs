using Blanquita.Data;
using Blanquita.Interfaces;
using Blanquita.Models;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Services
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly Context _context;
        public PrinterRepository(Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Cajas>> GetAllCajasAsync()
        {
            return await _context.Cajas.ToListAsync();
        }
        public async Task<Cajas> GetCajaByIdAsync(int id)
        {
            return await _context.Cajas.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
