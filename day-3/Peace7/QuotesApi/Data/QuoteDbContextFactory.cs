using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuotesApi.Data;

public class QuoteDbContextFactory
    : IDesignTimeDbContextFactory<QuoteDbContext>
{
    public QuoteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<QuoteDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=QuotesApi;Trusted_Connection=True;TrustServerCertificate=True");

        return new QuoteDbContext(optionsBuilder.Options);
    }
}