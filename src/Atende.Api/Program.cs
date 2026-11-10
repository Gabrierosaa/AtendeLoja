var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Fase 0: apenas o esqueleto. Endpoints reais entram a partir da fase 1.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
