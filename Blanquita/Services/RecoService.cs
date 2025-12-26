using Blanquita.Interfaces;
using Blanquita.Data;
using Blanquita.Models;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Blanquita.Services
{
    public class RecoService : IRecoService
    {
        private readonly Context _context;
        public RecoService(Context context)
        {
            _context = context;
        }
        public async Task<List<Recolecciones>> RecosPorFecha(DateTime fecha)
        {
            return await _context.Recolecciones
                .Where(c => c.FechaHora.Date == fecha.Date)
                .ToListAsync();
        }
        public async Task<Recolecciones> CrearRecoleccionAsync(Recolecciones recoleccion)
        {
            // Obtener el último folio para esta caja
            var ultimoFolio = await _context.Recolecciones
                .Where(r => r.Caja == recoleccion.Caja && !r.Corte)
                .OrderByDescending(r => r.Folio)
                .FirstOrDefaultAsync();
            // Si es la primera recolección o hubo un corte anterior, comenzar en 1
            recoleccion.Folio = ultimoFolio == null ? 1 : ultimoFolio.Folio + 1;
            recoleccion.Corte = false;

            _context.Recolecciones.Add(recoleccion);
            await _context.SaveChangesAsync();
            return recoleccion;
        }
        public async Task<int> HacerCorteAsync(string idCaja)
        {
            // Marcar todas las recolecciones de esta caja como "con corte"
            var recoleccionesSinCorte = await _context.Recolecciones
                .Where(r => r.Caja == idCaja && !r.Corte)
                .ToListAsync();

            foreach (var rec in recoleccionesSinCorte)
            {
                rec.Corte = true;
            }

            await _context.SaveChangesAsync();
            return recoleccionesSinCorte.Count;
        }
        public async Task<TableData<Recolecciones>> ObtenerRecosPaginadasAsync(TableState state, string searchString = null)
        {
            IQueryable<Recolecciones> query = _context.Recolecciones.AsQueryable();
            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Folio.ToString().Contains(searchString) ||
                    c.Caja.ToString().Contains(searchString) ||
                    c.Caja.ToString().Contains(searchString) ||
                    c.FechaHora.ToString().Contains(searchString));
            }
            // Obtener el total de items antes de paginar
            int totalItems = await query.CountAsync();
            // Aplicar ordenamiento
            switch (state.SortLabel)
            {
                case "Id":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Id)
                        : query.OrderByDescending(c => c.Id);
                    break;
                case "Folio":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Folio)
                        : query.OrderByDescending(c => c.Folio);
                    break;
                case "Fecha_Hora":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.FechaHora)
                        : query.OrderByDescending(c => c.FechaHora);
                    break;
                case "Caja":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Caja)
                        : query.OrderByDescending(c => c.Caja);
                    break;
                case "Cantidad":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.CantidadTotal)
                        : query.OrderByDescending(c => c.CantidadTotal);
                    break;
            }
            // Aplicar paginación
            var pagedData = await query
                .Skip(state.Page * state.PageSize)
                .Take(state.PageSize)
                .ToListAsync();
            return new TableData<Recolecciones>()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        public async Task<Recolecciones> BuscarRecoPorId (int id)
        {
            return await _context.Recolecciones.FirstOrDefaultAsync(ca => ca.Id == id);
        }
    }
}
