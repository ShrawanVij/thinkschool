using Testcontainers.MsSql;

namespace Quotes.Tests.Integration;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer =
        new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

    public string ConnectionString =>
        _sqlServer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
    }
}