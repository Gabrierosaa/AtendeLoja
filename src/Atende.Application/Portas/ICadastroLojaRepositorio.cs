using Atende.Domain.Entidades;

namespace Atende.Application.Portas
{
    public interface ICadastroLojaRepositorio
    {
        Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default);
        Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
        Task AdicionarAsync(Loja loja, Usuario dono, CancellationToken ct = default);
    }
}
