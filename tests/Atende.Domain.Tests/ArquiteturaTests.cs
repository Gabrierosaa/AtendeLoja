using System.Xml.Linq;

namespace Atende.Domain.Tests;

public class ArquiteturaTests
{
    [Fact]
    public void Domain_nao_referencia_nenhum_projeto()
    {
        var csproj = LocalizarCsprojDoDomain();

        var referencias = XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(e => (string?)e.Attribute("Include"))
            .ToList();

        Assert.True(
            referencias.Count == 0,
            $"Atende.Domain não pode ter ProjectReference, mas referencia: {string.Join(", ", referencias)}");
    }

    private static string LocalizarCsprojDoDomain()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidato = Path.Combine(dir.FullName, "src", "Atende.Domain", "Atende.Domain.csproj");
            if (File.Exists(candidato)) return candidato;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Não encontrei src/Atende.Domain/Atende.Domain.csproj subindo a partir de " + AppContext.BaseDirectory);
    }
}
