using System.Diagnostics;
using Atende.Application.Excecoes;
using Atende.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Atende.Api.Middlewares
{
    public class TratadorDeExcecoes : IExceptionHandler
    {
        private readonly ILogger<TratadorDeExcecoes> _log;

        public TratadorDeExcecoes(ILogger<TratadorDeExcecoes> log) => _log = log;

        public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
        {
            if (ex is OperationCanceledException && ct.IsCancellationRequested)
                return true;

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

                case DomainException d:
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Title = "Regra de negócio violada.",
                        Detail = d.Message,
                    };
                    break;

                case NaoEncontradoException n:
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Não encontrado.",
                        Detail = n.Message,
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

            problema.Type ??= $"https://httpstatuses.com/{problema.Status}";
            problema.Instance = ctx.Request.Path;
            problema.Extensions["traceId"] = Activity.Current?.Id ?? ctx.TraceIdentifier;

            ctx.Response.StatusCode = problema.Status!.Value;
            await ctx.Response.WriteAsJsonAsync(
                problema, problema.GetType(), options: null, contentType: "application/problem+json", cancellationToken: ct);
            return true;
        }
    }
}
