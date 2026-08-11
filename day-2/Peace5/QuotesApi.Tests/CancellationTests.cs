using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Tests;

public class CancellationTests
{
    [Fact]
    public async Task RequestCancellation_DoesNotComplete()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ICollectionRepository>();

                    services.AddScoped<ICollectionRepository, SlowCollectionRepository>();
                });
            });

        using var client = factory.CreateClient();

        using var cts = new CancellationTokenSource();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/collections/1/items")
        {
            Content = new StringContent(
                """{"quoteId":1}""",
                System.Text.Encoding.UTF8,
                "application/json")
        };

        var responseTask = client.SendAsync(
            request,
            cts.Token);

        // Give the request time to reach the repository.
        await Task.Delay(100);

        // Cancel while the repository is still working.
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await responseTask);
    }
}

public class SlowCollectionRepository : ICollectionRepository
{
    public async Task<Collection?> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        // Simulate a long-running database operation.
        await Task.Delay(
            TimeSpan.FromSeconds(10),
            cancellationToken);

        return new Collection(1, "Test Collection");
    }

    public Task Add(
        Collection collection,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Update(
        Collection collection,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Delete(
        Collection collection,
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}