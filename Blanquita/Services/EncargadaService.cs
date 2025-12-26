using Blanquita.Data;
using Blanquita.Models;
using Blanquita.Interfaces;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Blanquita.Services
{
    public class EncargadaService : IEncargadaService
    {
        private readonly Context _context;
        public EncargadaService(Context context)
        {
            _context = context;
        }
        public async Task<List<Encargadas>> ObtenerEncargadas()
        {
            return await _context.Encargadas.ToListAsync();
        }
        public async Task<Encargadas> CrearEncargadaAsync(Encargadas encargada)
        {
            _context.Encargadas.Add(encargada);
            await _context.SaveChangesAsync();
            return encargada;
        }
        public async Task<Encargadas> EncontrarEncargadaPorId(int id)
        {
            var encargada = await _context.Encargadas.FirstOrDefaultAsync(ca => ca.Id == id);
            return encargada;
        }
        public async Task<TableData<Encargadas>> ObtenerEncargadasPaginadasAsync(TableState state, string searchString = null)
        {
            IQueryable<Encargadas> query = _context.Encargadas.AsQueryable();

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(searchString) ||
                    c.Sucursal.ToString().Contains(searchString));
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

            return new TableData<Encargadas>()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        public async Task<Encargadas> ActualizarEncargadaAsync(Encargadas encargadaActualizada)
        {
            var encargadaExistente = await _context.Encargadas.FindAsync(encargadaActualizada.Id);

            if (encargadaExistente == null)
                throw new KeyNotFoundException("Encargada no encontrada");

            // Actualizar propiedades
            encargadaExistente.Nombre = encargadaActualizada.Nombre;
            encargadaExistente.Sucursal = encargadaActualizada.Sucursal;

            await _context.SaveChangesAsync();
            return encargadaExistente;
        }
    }
}
