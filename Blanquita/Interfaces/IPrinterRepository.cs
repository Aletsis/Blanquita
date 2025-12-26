using System.Collections.Generic;
using System.Threading.Tasks;
using Blanquita.Models;

namespace Blanquita.Interfaces
{
    public interface IPrinterRepository
    {
        Task<IEnumerable<Cajas>> GetAllCajasAsync();
        Task<Cajas> GetCajaByIdAsync(int id);
    }
}
