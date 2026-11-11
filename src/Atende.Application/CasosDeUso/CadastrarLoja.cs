using Atende.Application.Dtos;
using Atende.Application.Excecoes;
using Atende.Application.Portas;
using Atende.Application.Texto;
using Atende.Domain.Entidades;
using FluentValidation;

namespace Atende.Application.CasosDeUso
{
    public class CadastrarLoja
    {
        public const string FusoPadrao = "America/Sao_Paulo";
        public const string MensagemEncaminhamentoPadrao =
            "Não encontrei essa informação. Por favor, entre em contato diretamente com a loja.";

        private readonly IValidator<CadastrarLojaComando> _validador;
        private readonly ICadastroLojaRepositorio _repositorio;
        private readonly ISenhaHasher _hasher;

        public CadastrarLoja(
            IValidator<CadastrarLojaComando> validador,
            ICadastroLojaRepositorio repositorio,
            ISenhaHasher hasher)
        {
            _validador = validador;
            _repositorio = repositorio;
            _hasher = hasher;
        }

        public async Task<CadastrarLojaResposta> ExecutarAsync(
            CadastrarLojaComando comando, CancellationToken ct = default)
        {
            await _validador.ValidateAndThrowAsync(comando, ct);

            var nome = comando.Nome.Trim();
            var email = comando.Email.Trim().ToLowerInvariant();
            var slug = SlugGerador.Gerar(nome);
            var fuso = string.IsNullOrWhiteSpace(comando.FusoHorario)
                ? FusoPadrao
                : comando.FusoHorario.Trim();

            if (await _repositorio.ExisteSlugAsync(slug, ct))
                throw new ConflitoException($"Já existe uma loja com o endereço '{slug}'.");

            if (await _repositorio.ExisteEmailAsync(email, ct))
                throw new ConflitoException("Já existe um usuário com este e-mail.");

            var loja = new Loja(nome, slug, fuso, MensagemEncaminhamentoPadrao);
            var dono = new Usuario(loja.id, email, _hasher.Hash(comando.Senha), nome);

            await _repositorio.AdicionarAsync(loja, dono, ct);

            return new CadastrarLojaResposta(loja.id, dono.id, slug);
        }
    }
}
