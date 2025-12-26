using Blanquita.Models;
using MudBlazor;

namespace Blanquita.Interfaces
{
    public interface ICorteService
    {
        Task<Cortes> GuardarCorte(Cortes corte);
        Task<TableData<Cortes>> ObtenerCortesPaginados(TableState state, string searchString = null);
        Task<Cortes> BuscarCortePorId(int id);

    }
}
