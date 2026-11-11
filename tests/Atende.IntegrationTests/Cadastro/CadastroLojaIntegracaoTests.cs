using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atende.Api.Contratos;
using Atende.Application.Excecoes;
using Atende.Domain.Entidades;
using Atende.Infrastructure.Persistencia.Repositorios;
using Atende.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Atende.IntegrationTests.Cadastro;

[Collection(BancoCollection.Nome)]
public class CadastroLojaIntegracaoTests : IDisposable
{
    private const string Rota = "/api/v1/auth/registro";
    private const string SenhaValida = "senha-segura-123";

    private readonly SqlServerFixture _banco;
    private readonly ApiFactory _api;
    private readonly HttpClient _http;

    public CadastroLojaIntegracaoTests(SqlServerFixture banco)
    {
        _banco = banco;
        _api = new ApiFactory(banco.ConnectionString);
        _http = _api.CreateClient();
    }

    public void Dispose()
    {
        _http.Dispose();
        _api.Dispose();
    }

    // Nomes e e-mails únicos por teste: o banco é compartilhado entre os testes da coleção.
    private static string NomeUnico(string prefixo = "Padaria") => $"{prefixo} {Guid.NewGuid():N}";
    private static string EmailUnico() => $"{Guid.NewGuid():N}@exemplo.com";

    private Task<HttpResponseMessage> Registrar(string nome, string email, string senha = SenhaValida)
        => _http.PostAsJsonAsync(Rota, new RegistroRequest(nome, email, senha));

    [Fact]
    public async Task Cadastro_valido_retorna_201_e_grava_loja_e_dono_com_hash()
    {
        var nome = NomeUnico();
        var email = EmailUnico();

        var resposta = await Registrar(nome, email);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("hash", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(SenhaValida, corpo);

        var dto = JsonSerializer.Deserialize<RegistroResponse>(corpo, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        await using var db = _banco.CriarContexto();
        var loja = await db.Lojas.SingleAsync(l => l.id == dto.LojaId);
        var dono = await db.Usuarios.SingleAsync(u => u.id == dto.UsuarioId);

        Assert.Equal(dto.Slug, loja.slug);
        Assert.Equal(loja.id, dono.LojaId);
        Assert.Equal(email, dono.email);
        Assert.NotEqual(SenhaValida, dono.SenhaHash);
        Assert.False(string.IsNullOrWhiteSpace(dono.SenhaHash));
    }

    [Fact]
    public async Task Slug_e_gerado_sem_acento_a_partir_do_nome()
    {
        var sufixo = Guid.NewGuid().ToString("N");

        var resposta = await Registrar($"Padaria do Zé {sufixo}", EmailUnico());

        var dto = (await resposta.Content.ReadFromJsonAsync<RegistroResponse>())!;
        Assert.Equal($"padaria-do-ze-{sufixo}", dto.Slug);
    }

    [Fact]
    public async Task Email_duplicado_retorna_409_e_nao_grava_nada()
    {
        var email = EmailUnico();
        Assert.Equal(HttpStatusCode.Created, (await Registrar(NomeUnico(), email)).StatusCode);

        var nomeNovo = NomeUnico("Outra");
        var resposta = await Registrar(nomeNovo, email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        await using var db = _banco.CriarContexto();
        Assert.False(await db.Lojas.AnyAsync(l => l.nome == nomeNovo));
    }

    [Fact]
    public async Task Slug_duplicado_retorna_409_e_nao_grava_nada()
    {
        var nome = NomeUnico();
        Assert.Equal(HttpStatusCode.Created, (await Registrar(nome, EmailUnico())).StatusCode);

        var emailNovo = EmailUnico();
        var resposta = await Registrar(nome.ToUpperInvariant(), emailNovo); // mesmo slug

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        await using var db = _banco.CriarContexto();
        Assert.False(await db.Usuarios.AnyAsync(u => u.email == emailNovo));
    }

    [Fact]
    public async Task Senha_fraca_retorna_400_com_erro_no_campo()
    {
        var resposta = await Registrar(NomeUnico(), EmailUnico(), "1234567");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Senha", corpo);
    }

    [Fact]
    public async Task Falha_ao_gravar_o_dono_desfaz_a_loja_rollback()
    {
        var emailExistente = EmailUnico();
        Assert.Equal(HttpStatusCode.Created, (await Registrar(NomeUnico(), emailExistente)).StatusCode);

        var slugNovo = $"rollback-{Guid.NewGuid():N}";
        var loja = new Loja("Loja Rollback", slugNovo, "America/Sao_Paulo", "msg");
        var dono = new Usuario(loja.id, emailExistente, "hash", "Dono");

        await using (var db = _banco.CriarContexto())
        {
            var repo = new CadastroLojaRepositorio(db);
            await Assert.ThrowsAsync<ConflitoException>(() => repo.AdicionarAsync(loja, dono));
        }

        await using var verificacao = _banco.CriarContexto();
        Assert.False(await verificacao.Lojas.AnyAsync(l => l.slug == slugNovo),
            "A loja foi gravada sem o dono: a transação não foi desfeita.");
    }
}
