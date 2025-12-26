using Blanquita.Data;
using Blanquita.Models;
using Blanquita.Interfaces;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Blanquita.Services
{
    public class CajaService : ICajaService
    {
        private readonly Context _context;
        public CajaService(Context context)
        {
            _context = context;
        }
        public async Task<List<Cajas>> ObtenerCajas()
        {
            return await _context.Cajas.ToListAsync();
        }

        public async Task<Cajas> CrearCajaAsync(Cajas caja)
        {
            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync();
            return caja;
        }
        public async Task<Cajas> EncontrarCajaPorId(int id)
        {
            var caja = await _context.Cajas.FirstOrDefaultAsync(ca => ca.Id == id);
            return caja;
        }
        public async Task<List<Cajas>> EncontrarCajasPorSucursal(int sucursal)
        {
            var cajas = await _context.Cajas.Where(c => c.Sucursal == sucursal).ToListAsync();
            return cajas;
        }
        public async Task<TableData<Cajas>> ObtenerCajasPaginadasAsync(TableState state, string searchString = null)
        {
            IQueryable<Cajas> query = _context.Cajas.AsQueryable();

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(searchString) ||
                    c.IpImpresora.Contains(searchString));
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
                case "IpImpresora":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.IpImpresora)
                        : query.OrderByDescending(c => c.IpImpresora);
                    break;
                case "PuertoImpresora":
                    query = state.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(c => c.Port)
                        : query.OrderByDescending(c => c.Port);
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

            return new TableData<Cajas>()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        public async Task<Cajas> ActualizarCajaAsync(Cajas cajaActualizada)
        {
            var cajaExistente = await _context.Cajas.FindAsync(cajaActualizada.Id);

            if (cajaExistente == null)
                throw new KeyNotFoundException("Caja no encontrada");

            // Actualizar propiedades
            cajaExistente.Nombre = cajaActualizada.Nombre;
            cajaExistente.IpImpresora = cajaActualizada.IpImpresora;
            cajaExistente.Port = cajaActualizada.Port;
            cajaExistente.Sucursal = cajaActualizada.Sucursal;
            cajaExistente.Ultima = cajaActualizada.Ultima;

            await _context.SaveChangesAsync();
            return cajaExistente;
        }
    }
}
