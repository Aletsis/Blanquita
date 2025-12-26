using Blanquita.Models;
using MudBlazor;

namespace Blanquita.Interfaces
{
    public interface IEncargadaService
    {
        Task<List<Encargadas>> ObtenerEncargadas();
        Task<Encargadas> CrearEncargadaAsync(Encargadas encargada);
        Task<Encargadas> EncontrarEncargadaPorId(int id);
        Task<TableData<Encargadas>> ObtenerEncargadasPaginadasAsync(TableState state, string searchString = null);
        Task<Encargadas> ActualizarEncargadaAsync(Encargadas encargadaActualizada);
    }
}
