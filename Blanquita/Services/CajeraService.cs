using Blanquita.Data;
using Blanquita.Interfaces;
using Blanquita.Models;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Blanquita.Services
{
    public class CajeraService : ICajeraService
    {
        private readonly Context _context;
        public CajeraService(Context context)
        {
            _context = context;
        }
        public async Task<List<Cajeras>> ObtenerCajeras()
        {
            return await _context.Cajeras.ToListAsync();
        }
        public async Task<Cajeras> CrearCajeraAsync(Cajeras cajera)
        {
            _context.Cajeras.Add(cajera);
            await _context.SaveChangesAsync();
            return cajera;
        }
        public async Task<Cajeras> EncontrarCajeraPorId(int id)
        {
            var cajera = await _context.Cajeras.FirstOrDefaultAsync(ca => ca.Id == id);
            return cajera;
        }
        public async Task<List<Cajeras>> EncontrarCajerasPorSucursal(int sucursal)
        {
            var cajeras = await _context.Cajeras.Where(c => c.Sucursal == sucursal).ToListAsync();
            return cajeras;
        }
        public async Task<TableData<Cajeras>> ObtenerCajerasPaginadasAsync(TableState state, string searchString = null)
        {
            IQueryable<Cajeras> query = _context.Cajeras.AsQueryable();

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(searchString) ||
                    c.NumNomina.ToString().Contains(searchString));
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
                case "Nomina":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.NumNomina)
                        : query.OrderByDescending(c => c.NumNomina);
                    break;
                case "Nombre":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Nombre)
                        : query.OrderByDescending(c => c.Nombre);
                    break;
                case "Sucursal":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Sucursal)
                        : query.OrderByDescending(c => c.Sucursal);
                    break;
            }

            // Aplicar paginación
            var pagedData = await query
                .Skip(state.Page * state.PageSize)
                .Take(state.PageSize)
                .ToListAsync();

            return new TableData<Cajeras>()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        public async Task<Cajeras> ActualizarCajeraAsync(Cajeras cajeraActualizada)
        {
            var cajeraExistente = await _context.Cajeras.FindAsync(cajeraActualizada.Id);

            if (cajeraExistente == null)
                throw new KeyNotFoundException("Cajera no encontrada");

            // Actualizar propiedades
            cajeraExistente.Nombre = cajeraActualizada.Nombre;
            cajeraExistente.NumNomina = cajeraActualizada.NumNomina;
            cajeraExistente.Sucursal = cajeraActualizada.Sucursal;
            cajeraExistente.Edo = cajeraActualizada.Edo;

            await _context.SaveChangesAsync();
            return cajeraExistente;
        }
    }
}
