using Atende.Application.CasosDeUso;
using Atende.Application.Dtos;
using Atende.Application.Excecoes;
using Atende.Application.Portas;
using Atende.Application.Validacao;
using Atende.Domain.Entidades;
using FluentValidation;

namespace Atende.Application.Tests.CasosDeUso;

public class CadastrarLojaTests
{
    private sealed class RepoFalso : ICadastroLojaRepositorio
    {
        public HashSet<string> Slugs { get; } = new();
        public HashSet<string> Emails { get; } = new();
        public List<(Loja Loja, Usuario Dono)> Gravados { get; } = new();

        public Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default) => Task.FromResult(Slugs.Contains(slug));
        public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Emails.Contains(email));
        public Task AdicionarAsync(Loja loja, Usuario dono, CancellationToken ct = default)
        {
            Gravados.Add((loja, dono));
            return Task.CompletedTask;
        }
    }

    private sealed class HasherFalso : ISenhaHasher
    {
        public string Hash(string senha) => "HASH:" + senha.Length;
    }

    private readonly RepoFalso _repo = new();

    private CadastrarLoja Criar() => new(new CadastrarLojaValidador(), _repo, new HasherFalso());

    private static CadastrarLojaComando Valido(string nome = "Padaria do Zé") =>
        new(nome, "Ze@Padaria.com ", "senha-segura-123");

    [Fact]
    public async Task Cadastro_valido_grava_loja_e_dono_com_hash_e_slug()
    {
        var resposta = await Criar().ExecutarAsync(Valido());

        Assert.Equal("padaria-do-ze", resposta.Slug);
        var (loja, dono) = Assert.Single(_repo.Gravados);
        Assert.Equal(loja.id, dono.LojaId);
        Assert.Equal("America/Sao_Paulo", loja.FusoHorario);
        Assert.Equal("ze@padaria.com", dono.email);
        Assert.NotEqual("senha-segura-123", dono.SenhaHash);
        Assert.StartsWith("HASH:", dono.SenhaHash);
    }

    [Fact]
    public async Task Senha_curta_gera_erro_de_validacao_no_campo_Senha()
    {
        var comando = Valido() with { Senha = "1234567" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Criar().ExecutarAsync(comando));

        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(CadastrarLojaComando.Senha));
        Assert.Empty(_repo.Gravados);
    }

    [Theory]
    [InlineData("", "a@b.com")]
    [InlineData("!!!", "a@b.com")]
    [InlineData("Loja", "nao-e-email")]
    public async Task Entradas_invalidas_nao_gravam_nada(string nome, string email)
    {
        var comando = new CadastrarLojaComando(nome, email, "senha-segura-123");

        await Assert.ThrowsAsync<ValidationException>(() => Criar().ExecutarAsync(comando));
        Assert.Empty(_repo.Gravados);
    }

    [Fact]
    public async Task Fuso_inexistente_e_rejeitado()
    {
        var comando = Valido() with { FusoHorario = "Marte/Olympus" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Criar().ExecutarAsync(comando));
        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(CadastrarLojaComando.FusoHorario));
    }

    [Fact]
    public async Task Slug_repetido_gera_conflito_e_nao_grava()
    {
        _repo.Slugs.Add("padaria-do-ze");

        await Assert.ThrowsAsync<ConflitoException>(() => Criar().ExecutarAsync(Valido()));
        Assert.Empty(_repo.Gravados);
    }

    [Fact]
    public async Task Email_repetido_gera_conflito_ignorando_caixa_e_espacos()
    {
        _repo.Emails.Add("ze@padaria.com");

        await Assert.ThrowsAsync<ConflitoException>(() => Criar().ExecutarAsync(Valido()));
        Assert.Empty(_repo.Gravados);
    }
}
