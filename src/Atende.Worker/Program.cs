var builder = Host.CreateApplicationBuilder(args);

// Fase 0: apenas o esqueleto. Os serviços de ingestão entram na fase 2.
var host = builder.Build();
host.Run();
