using Blanquita.Models;
using MudBlazor;

namespace Blanquita.Interfaces
{
    public interface ICajeraService
    {
        Task<List<Cajeras>> ObtenerCajeras();
        Task<Cajeras> CrearCajeraAsync(Cajeras cajera);
        Task<Cajeras> EncontrarCajeraPorId(int id);
        Task<List<Cajeras>> EncontrarCajerasPorSucursal(int sucursal);
        Task<TableData<Cajeras>> ObtenerCajerasPaginadasAsync(TableState state, string searchString = null);
        Task<Cajeras> ActualizarCajeraAsync(Cajeras cajeraActualizada);
    }
}
