using QuotesApi.Models;
using QuotesApi.Repositories;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class CollectionEndpoints
{
    public static void MapCollectionEndpoints(this WebApplication app)
    {
        app.MapPost("/collections", async (
            CreateCollectionRequest request,
            ICollectionRepository repository) =>
        {
            var collection = new Collection(request.OwnerId, request.Name);

            await repository.Add(collection);

            return Results.Ok(collection);
        });

        app.MapPost("/collections/{id}/items", async (
            int id,
            AddCollectionItemRequest request,
            ICollectionRepository repository,
            IClock clock) =>
        {
            var collection = await repository.GetById(id);

            if (collection is null)
                return Results.NotFound();

            collection.AddItem(request.QuoteId, clock);

            await repository.Update(collection);

            return Results.Ok(collection);
        });

        app.MapDelete("/collections/{id}/items/{quoteId}", async (
            int id,
            int quoteId,
            ICollectionRepository repository) =>
        {
            var collection = await repository.GetById(id);

            if (collection is null)
                return Results.NotFound();

            collection.RemoveItem(quoteId);

            await repository.Update(collection);

            return Results.Ok(collection);
        });
    }

    public record CreateCollectionRequest(int OwnerId, string Name);

    public record AddCollectionItemRequest(int QuoteId);
}