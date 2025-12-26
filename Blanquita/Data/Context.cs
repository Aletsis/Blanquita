using Blanquita.Models;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        public DbSet<Cajeras> Cajeras { get; set; }
        public DbSet<Cajas> Cajas { get; set; }
        public DbSet<Encargadas> Encargadas { get; set; }
        public DbSet<Recolecciones> Recolecciones { get; set; }
        public DbSet<Cortes> Cortes { get; set; }
    }
}
