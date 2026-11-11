using Atende.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Atende.IntegrationTests.Infra;

public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CriarContexto();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AtendeDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AtendeDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new AtendeDbContext(options);
    }
}

[CollectionDefinition(Nome)]
public class BancoCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Nome = "Banco SQL Server";
}
