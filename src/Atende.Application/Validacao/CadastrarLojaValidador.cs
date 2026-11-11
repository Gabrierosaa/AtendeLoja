using Atende.Application.Dtos;
using Atende.Application.Texto;
using FluentValidation;

namespace Atende.Application.Validacao
{
    public class CadastrarLojaValidador : AbstractValidator<CadastrarLojaComando>
    {
        public const int SenhaMinima = 8;
        public const int SenhaMaxima = 128;

        public CadastrarLojaValidador()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O nome da loja é obrigatório.")
                .MaximumLength(150).WithMessage("O nome da loja deve ter no máximo 150 caracteres.")
                .Must(n => SlugGerador.Gerar(n).Length > 0)
                    .WithMessage("O nome da loja deve conter ao menos uma letra ou número.")
                    .When(c => !string.IsNullOrWhiteSpace(c.Nome), ApplyConditionTo.CurrentValidator);

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .MaximumLength(200).WithMessage("O e-mail deve ter no máximo 200 caracteres.")
                .EmailAddress().WithMessage("O e-mail informado não é válido.");

            RuleFor(c => c.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(SenhaMinima).WithMessage($"A senha deve ter no mínimo {SenhaMinima} caracteres.")
                .MaximumLength(SenhaMaxima).WithMessage($"A senha deve ter no máximo {SenhaMaxima} caracteres.");

            RuleFor(c => c.FusoHorario)
                .MaximumLength(60).WithMessage("O fuso horário deve ter no máximo 60 caracteres.")
                .Must(FusoExiste).WithMessage("O fuso horário informado não existe.")
                .When(c => !string.IsNullOrWhiteSpace(c.FusoHorario));
        }

        private static bool FusoExiste(string? id)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(id!.Trim());
                return true;
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                return false;
            }
        }
    }
}
