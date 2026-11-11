using System.Threading.RateLimiting;
using Atende.Api.Middlewares;
using Atende.Application.CasosDeUso;
using Atende.Application.Dtos;
using Atende.Application.Validacao;
using Atende.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// O ASP.NET só lê o user-secrets em Development. Carregamos sempre (opcional): em produção
// o cofre não existe e nada muda; na máquina de desenvolvimento a connection string funciona
// mesmo se a IDE iniciar a API com outro ASPNETCORE_ENVIRONMENT.
// Só quando nada mais definiu a string (variável de ambiente, testes, Docker): ela tem prioridade.
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Padrao")))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);

    // Alternativa ao user-secrets: arquivo local fora do Git (ver .gitignore e README).
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Padrao")))
        builder.Configuration.AddJsonFile(
            Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json"),
            optional: true, reloadOnChange: false);
}

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Padrao")))
    throw new InvalidOperationException(
        $"ConnectionStrings:Padrao não definida. Ambiente='{builder.Environment.EnvironmentName}', " +
        $"ContentRoot='{builder.Environment.ContentRootPath}'. Defina por variável de ambiente, " +
        "user-secrets ou appsettings.Local.json (veja o README).");

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
            PermitLimit = builder.Configuration.GetValue("RateLimit:Registro:Limite", 5),
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
