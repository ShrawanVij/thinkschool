using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuotesApi.Data;
using QuotesApi.Services;

namespace Quotes.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqlServerFixture _fixture;

    public CustomWebApplicationFactory(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<QuoteDbContext>();
            services.RemoveAll<DbContextOptions<QuoteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<QuoteDbContext>>();

            services.AddDbContext<QuoteDbContext>(options =>
            {
                options.UseSqlServer(
                    _fixture.ConnectionString);
                
                options.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(
                        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId
                            .PendingModelChangesWarning);
                });
            });
        });
    }
}