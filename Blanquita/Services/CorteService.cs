using Blanquita.Interfaces;
using Blanquita.Data;
using Blanquita.Models;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Blanquita.Services
{
    public class CorteService : ICorteService
    {
        private readonly Context _context;
        public CorteService(Context context)
        {
            _context = context;
        }
        public async Task<Cortes> GuardarCorte (Cortes corte)
        {
            _context.Cortes.Add(corte);
            await _context.SaveChangesAsync();
            return corte;
        }
        public async Task<TableData<Cortes>> ObtenerCortesPaginados(TableState state, string searchString = null)
        {
            IQueryable<Cortes> query = _context.Cortes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Caja.Contains(searchString) ||
                    c.Encargada.Contains(searchString) ||
                    c.Cajera.Contains(searchString) ||
                    c.FechaHora.ToString().Contains(searchString));
            }
            int totalItems = await query.CountAsync();
            switch (state.SortLabel)
            {
                case "Id":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Id)
                        : query.OrderByDescending(c => c.Id);
                    break;
                case "Caja":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Caja)
                        : query.OrderByDescending(c => c.Caja);
                    break;
                case "Cajera":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Cajera)
                        : query.OrderByDescending(c => c.Cajera);
                    break;
                case "Sucursal":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Sucursal)
                        : query.OrderByDescending(c => c.Sucursal);
                    break;
                case "Fecha_Hora":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.FechaHora)
                        : query.OrderByDescending(c => c.FechaHora);
                    break;
            }
            var pagedData = await query
                .Skip(state.Page * state.PageSize)
                .Take(state.PageSize)
                .ToListAsync();
            return new TableData<Cortes>()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        public async Task<Cortes> BuscarCortePorId(int id)
        {
            var corte = await _context.Cortes.FirstOrDefaultAsync(c => c.Id == id);
            return corte;
        }
    }
}
