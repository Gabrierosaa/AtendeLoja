using Atende.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Atende.Infrastructure.Persistencia
{
    public class AtendeDbContext : DbContext
    {
        public AtendeDbContext(DbContextOptions<AtendeDbContext> options) : base(options) { }

        public DbSet<Loja> Lojas => Set<Loja>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtendeDbContext).Assembly);
        }
    }
}
