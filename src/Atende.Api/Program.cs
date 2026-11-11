using System.Threading.RateLimiting;
using Atende.Api.Middlewares;
using Atende.Application.CasosDeUso;
using Atende.Application.Dtos;
using Atende.Application.Validacao;
using Atende.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeExcecoes>();

builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AddScoped<IValidator<CadastrarLojaComando>, CadastrarLojaValidador>();
builder.Services.AddScoped<CadastrarLoja>();

// Rota pública de cadastro: limite por IP.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("registro", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
        }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

public partial class Program { }
