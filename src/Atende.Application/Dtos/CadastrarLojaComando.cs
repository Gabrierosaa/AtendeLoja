namespace Atende.Application.Dtos
{
    public record CadastrarLojaComando(
        string Nome,
        string Email,
        string Senha,
        string? FusoHorario = null);

    public record CadastrarLojaResposta(Guid LojaId, Guid UsuarioId, string Slug);
}
