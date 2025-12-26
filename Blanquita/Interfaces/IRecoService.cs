using Blanquita.Models;
using MudBlazor;

namespace Blanquita.Interfaces
{
    public interface IRecoService
    {
        Task<List<Recolecciones>> RecosPorFecha(DateTime fecha);
        Task<Recolecciones> CrearRecoleccionAsync(Recolecciones recoleccion);
        Task<int> HacerCorteAsync(string idCaja);
        Task<TableData<Recolecciones>> ObtenerRecosPaginadasAsync(TableState state, string searchString = null);
        Task<Recolecciones> BuscarRecoPorId(int id);
    }
}
