using Atende.Application.Excecoes;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Atende.Api.Middlewares
{
    /// <summary>Traduz exceções da aplicação para ProblemDetails (RFC 9457).</summary>
    public class TratadorDeExcecoes : IExceptionHandler
    {
        private readonly ILogger<TratadorDeExcecoes> _log;

        public TratadorDeExcecoes(ILogger<TratadorDeExcecoes> log) => _log = log;

        public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
        {
            ProblemDetails problema;

            switch (ex)
            {
                case ValidationException v:
                    var erros = v.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                    problema = new ValidationProblemDetails(erros)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Dados inválidos.",
                    };
                    break;

                case ConflitoException c:
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Conflito.",
                        Detail = c.Message,
                    };
                    break;

                default:
                    _log.LogError(ex, "Erro não tratado");
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Erro interno.",
                    };
                    break;
            }

            ctx.Response.StatusCode = problema.Status!.Value;
            await ctx.Response.WriteAsJsonAsync(
                problema, problema.GetType(), options: null, contentType: "application/problem+json", cancellationToken: ct);
            return true;
        }
    }
}
