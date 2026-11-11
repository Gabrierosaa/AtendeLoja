using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atende.Infrastructure.Persistencia
{
    /// <summary>
    /// Usada apenas pelas ferramentas do EF (dotnet ef) em tempo de design.
    /// Lê a connection string de ConnectionStrings__Padrao.
    /// </summary>
    public class AtendeDbContextFactory : IDesignTimeDbContextFactory<AtendeDbContext>
    {
        public AtendeDbContext CreateDbContext(string[] args)
        {
            var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__Padrao")
                ?? throw new InvalidOperationException(
                    "Defina a variável de ambiente ConnectionStrings__Padrao para usar o dotnet ef.");

            var options = new DbContextOptionsBuilder<AtendeDbContext>()
                .UseSqlServer(conexao)
                .Options;

            return new AtendeDbContext(options);
        }
    }
}
