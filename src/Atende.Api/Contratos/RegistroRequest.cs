namespace Atende.Api.Contratos
{
    /// <summary>Corpo de POST /api/v1/auth/registro.</summary>
    public record RegistroRequest(string Nome, string Email, string Senha, string? FusoHorario = null);

    /// <summary>Resposta do cadastro. Nunca contém senha nem hash.</summary>
    public record RegistroResponse(Guid LojaId, Guid UsuarioId, string Slug);
}
