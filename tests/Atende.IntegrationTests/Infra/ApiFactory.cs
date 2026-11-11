using Microsoft.AspNetCore.Mvc.Testing;

namespace Atende.IntegrationTests.Infra;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Padrao", _connectionString);
        builder.UseSetting("RateLimit:Registro:Limite", "10000");
    }
}
