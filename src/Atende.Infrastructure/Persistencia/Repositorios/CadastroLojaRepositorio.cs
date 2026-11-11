using Atende.Application.Excecoes;
using Atende.Application.Portas;
using Atende.Domain.Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atende.Infrastructure.Persistencia.Repositorios
{
    public class CadastroLojaRepositorio : ICadastroLojaRepositorio
    {
        private readonly AtendeDbContext _db;

        public CadastroLojaRepositorio(AtendeDbContext db) => _db = db;

        public Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default)
            => _db.Lojas.AnyAsync(l => l.slug == slug, ct);

        public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
            => _db.Usuarios.AnyAsync(u => u.email == email, ct);

        public async Task AdicionarAsync(Loja loja, Usuario dono, CancellationToken ct = default)
        {
            _db.Lojas.Add(loja);
            _db.Usuarios.Add(dono);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ViolouIndiceUnico(ex, "UQ_Loja_Slug"))
            {
                throw new ConflitoException($"Já existe uma loja com o endereço '{loja.slug}'.");
            }
            catch (DbUpdateException ex) when (ViolouIndiceUnico(ex, "UQ_Usuario_Email"))
            {
                throw new ConflitoException("Já existe um usuário com este e-mail.");
            }
        }

        private static bool ViolouIndiceUnico(DbUpdateException ex, string indice)
            => ex.InnerException is SqlException sql
               && (sql.Number == 2601 || sql.Number == 2627)
               && sql.Message.Contains(indice, StringComparison.OrdinalIgnoreCase);
    }
}
