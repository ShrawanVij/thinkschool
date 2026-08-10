using Microsoft.EntityFrameworkCore;
using QuotesApi.Models;

namespace QuotesApi.Data;

public class QuoteDbContext : DbContext
{
    public QuoteDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>()
            .OwnsMany(c => c.Items, builder =>
            {
                builder.HasKey("CollectionId", nameof(CollectionItem.QuoteId));

                builder.Property(x => x.QuoteId)
                    .ValueGeneratedNever();
            });
    }
}