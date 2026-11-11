using Atende.Application.Texto;

namespace Atende.Application.Tests.Texto;

public class SlugGeradorTests
{
    [Theory]
    [InlineData("Padaria do João", "padaria-do-joao")]
    [InlineData("  Café & Cia!  ", "cafe-cia")]
    [InlineData("Ação   Ñandú -- 2000", "acao-nandu-2000")]
    [InlineData("---Loja---", "loja")]
    [InlineData("LOJA 24H", "loja-24h")]
    public void Gera_slug_sem_acento_e_com_hifens(string entrada, string esperado)
        => Assert.Equal(esperado, SlugGerador.Gerar(entrada));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!! ???")]
    [InlineData("日本語")]
    public void Retorna_vazio_quando_nao_sobra_caractere_valido(string? entrada)
        => Assert.Equal(string.Empty, SlugGerador.Gerar(entrada));

    [Fact]
    public void Respeita_tamanho_maximo_e_nao_termina_em_hifen()
    {
        var slug = SlugGerador.Gerar("aaaa bbbb", 5); // "aaaa-" -> "aaaa"
        Assert.Equal("aaaa", slug);

        var longo = SlugGerador.Gerar(string.Join(' ', Enumerable.Repeat("palavra", 30)));
        Assert.True(longo.Length <= SlugGerador.TamanhoMaximo);
        Assert.DoesNotContain("--", longo);
        Assert.False(longo.EndsWith('-'));
    }

    [Fact]
    public void E_deterministica()
        => Assert.Equal(SlugGerador.Gerar("Loja Teste"), SlugGerador.Gerar("Loja Teste"));
}
