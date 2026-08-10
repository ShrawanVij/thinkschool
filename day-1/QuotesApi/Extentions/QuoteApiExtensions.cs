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
    Quote quote,
    IQuoteRepository repository,
    CancellationToken cancellationToken,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("QuoteApi");

    if (string.IsNullOrWhiteSpace(quote.Author) ||
        string.IsNullOrWhiteSpace(quote.Text))
    {
        logger.LogWarning(
            "Invalid quote submitted: author or text is missing.");

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(quote.Author))
        {
            errors["author"] = new[]
            {
                "Author is required."
            };
        }

        if (string.IsNullOrWhiteSpace(quote.Text))
        {
            errors["text"] = new[]
            {
                "Text is required."
            };
        }

        return Results.ValidationProblem(errors);
    }

    if (quote.Author.Length > 100)
    {
        logger.LogWarning(
            "Invalid quote submitted: author exceeds 100 characters.");

        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["author"] = new[]
                {
                    "Author cannot exceed 100 characters."
                }
            });
    }

    if (quote.Text.Length > 1000)
    {
        logger.LogWarning(
            "Invalid quote submitted: text exceeds 1000 characters.");

        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["text"] = new[]
                {
                    "Text cannot exceed 1000 characters."
                }
            });
    }

    quote.Id = 0;

    var createdQuote = await repository.AddAsync(
        quote,
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
            var deleted = await repository.DeleteAsync(
                id,
                cancellationToken);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        });

        return app;
    }
}