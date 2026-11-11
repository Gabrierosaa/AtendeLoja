using System.Globalization;
using System.Text;

namespace Atende.Application.Texto
{
    /// <summary>
    /// Gera slugs para URL: minúsculas, sem acento, só [a-z0-9] separados por hífen.
    /// Função pura: sem acesso a banco, relógio ou aleatoriedade.
    /// </summary>
    public static class SlugGerador
    {
        /// <summary>Tamanho máximo da coluna Loja.Slug.</summary>
        public const int TamanhoMaximo = 80;

        /// <summary>
        /// Converte o texto em slug. Retorna string vazia se não sobrar nenhum
        /// caractere válido (o chamador decide o que fazer).
        /// </summary>
        public static string Gerar(string? texto, int tamanhoMaximo = TamanhoMaximo)
        {
            if (string.IsNullOrWhiteSpace(texto) || tamanhoMaximo <= 0)
                return string.Empty;

            var decomposto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposto.Length);
            var separadorPendente = false;

            foreach (var c in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue; // remove o acento, mantém a letra base

                var minuscula = char.ToLowerInvariant(c);
                if (minuscula is >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    if (separadorPendente && sb.Length > 0)
                        sb.Append('-');
                    separadorPendente = false;
                    sb.Append(minuscula);
                }
                else
                {
                    separadorPendente = true;
                }
            }

            var slug = sb.ToString();
            if (slug.Length > tamanhoMaximo)
                slug = slug[..tamanhoMaximo].TrimEnd('-');

            return slug;
        }
    }
}
