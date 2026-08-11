using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace QuotesApi.Extensions;

public static class QuoteApiExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddDbContext<QuoteDbContext>(options =>
            options.UseSqlite("Data Source=quotes.db"));

        services.AddScoped<IQuoteRepository, QuoteRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapQuoteEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/quotes", async (
    IQuoteRepository repository,
    CancellationToken cancellationToken,
    ILoggerFactory loggerFactory,
    int? page,
    int? size) =>
{
    var logger = loggerFactory.CreateLogger("QuoteApi");
    var currentPage = page ?? 1;
    var currentSize = size ?? 10;

    if (currentPage < 1 || currentSize < 1 || currentSize > 100)
    {
        logger.LogWarning(
            "Invalid pagination requested: page={Page}, size={Size}",
            currentPage,
            currentSize);

        return Results.BadRequest(
            "Page must be >= 1 and size must be between 1 and 100.");
    }

    logger.LogInformation(
        "Fetching quotes: page={Page}, size={Size}",
        currentPage,
        currentSize);

    var quotes = await repository.GetQuotesAsync(
        currentPage,
        currentSize,
        cancellationToken);

    logger.LogInformation(
        "Fetched {Count} quotes",
        quotes.Count);

    return Results.Ok(quotes);
});

        app.MapGet("/api/quotes/{id:int}", async (
            int id,
            IQuoteRepository repository,
            CancellationToken cancellationToken) =>
        {
            var quote = await repository.GetByIdAsync(
                id,
                cancellationToken);

            return quote is null
                ? Results.NotFound()
                : Results.Ok(quote);
        });

        app.MapPost("/api/quotes", async (
            CreateQuoteRequest request,
            IQuoteRepository repository,
            CancellationToken cancellationToken,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("QuoteApi");

            var result = Quote.Create(
                request.Author,
                request.Text);

            if (result.Error is not null)
            {
                logger.LogWarning(
                    "Invalid quote submitted: {Code}",
                    result.Error.Code);

                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["quote"] = new[]
                        {
                            result.Error.Message
                        }
                    });
            }

            var createdQuote = await repository.AddAsync(
                result.Quote!,
                cancellationToken);

            logger.LogInformation(
                "Created quote with ID {QuoteId}",
                createdQuote.Id);

            return Results.Created(
                $"/api/quotes/{createdQuote.Id}",
                createdQuote);
        });

        app.MapDelete("/api/quotes/{id:int}", async (
            int id,
            IQuoteRepository repository,
            CancellationToken cancellationToken) =>
        {
            var quote = await repository.GetByIdForUpdateAsync(
                id,
                cancellationToken);

            if (quote is null)
                return Results.NotFound();

            quote.SoftDelete();

            await repository.SaveAsync(cancellationToken);

            return Results.NoContent();
        });

        return app;
    }
    public record CreateQuoteRequest(string Author, string Text);
}