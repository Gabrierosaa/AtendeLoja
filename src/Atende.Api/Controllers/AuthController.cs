using Atende.Api.Contratos;
using Atende.Application.CasosDeUso;
using Atende.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atende.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("registro")]
        [EnableRateLimiting("registro")]
        [ProducesResponseType<RegistroResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Registrar(
            [FromBody] RegistroRequest request,
            [FromServices] CadastrarLoja cadastrarLoja,
            CancellationToken ct)
        {
            var resultado = await cadastrarLoja.ExecutarAsync(
                new CadastrarLojaComando(request.Nome, request.Email, request.Senha, request.FusoHorario), ct);

            var resposta = new RegistroResponse(resultado.LojaId, resultado.UsuarioId, resultado.Slug);
            return Created($"/api/v1/lojas/{resultado.Slug}", resposta);
        }
    }
}
