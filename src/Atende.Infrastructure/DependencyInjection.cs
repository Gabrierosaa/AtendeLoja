using Atende.Application.Portas;
using Atende.Infrastructure.Persistencia;
using Atende.Infrastructure.Persistencia.Repositorios;
using Atende.Infrastructure.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atende.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AdicionarInfraestrutura(
            this IServiceCollection services, IConfiguration configuracao)
        {
            var conexao = configuracao.GetConnectionString("Padrao")
                ?? throw new InvalidOperationException("Defina ConnectionStrings:Padrao.");

            services.AddDbContext<AtendeDbContext>(o => o.UseSqlServer(conexao));
            services.AddScoped<ICadastroLojaRepositorio, CadastroLojaRepositorio>();
            services.AddSingleton<ISenhaHasher, SenhaHasher>();

            return services;
        }
    }
}
