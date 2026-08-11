using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly QuoteDbContext _db;

    public QuoteRepository(QuoteDbContext db)
    {
        _db = db;
    }

    public async Task<List<Quote>> GetQuotesAsync(
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        return await _db.Quotes
            .AsNoTracking()
            .Where(q => !q.IsDeleted)
            .OrderBy(q => q.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public async Task<Quote?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Quotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                q => q.Id == id && !q.IsDeleted,
                cancellationToken);
    }

    public async Task<Quote> AddAsync(
        Quote quote,
        CancellationToken cancellationToken)
    {
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(cancellationToken);

        return quote;
    }

    public async Task<Quote?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Quotes
            .FirstOrDefaultAsync(
                q => q.Id == id,
                cancellationToken);
    }

    public async Task<bool> SaveAsync(
        CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}