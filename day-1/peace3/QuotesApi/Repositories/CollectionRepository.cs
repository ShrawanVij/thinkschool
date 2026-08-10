using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly QuoteDbContext _db;

    public CollectionRepository(QuoteDbContext db)
    {
        _db = db;
    }

    public async Task<Collection?> GetById(int id)
    {
        return await _db.Collections
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Add(Collection collection)
    {
        await _db.Collections.AddAsync(collection);
        await _db.SaveChangesAsync();
    }

    public async Task Update(Collection collection)
    {
        _db.Collections.Update(collection);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(Collection collection)
    {
        _db.Collections.Remove(collection);
        await _db.SaveChangesAsync();
    }
}