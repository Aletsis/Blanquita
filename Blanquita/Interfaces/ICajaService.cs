using Blanquita.Models;
using MudBlazor;

namespace Blanquita.Interfaces
{
    public interface ICajaService
    {
        Task<List<Cajas>> ObtenerCajas();
        Task<Cajas> CrearCajaAsync(Cajas caja);
        Task<Cajas> EncontrarCajaPorId(int id);
        Task<List<Cajas>> EncontrarCajasPorSucursal(int sucursal);
        Task<TableData<Cajas>> ObtenerCajasPaginadasAsync(TableState state, string searchString = null);
        Task<Cajas> ActualizarCajaAsync(Cajas caja);
    }
}
