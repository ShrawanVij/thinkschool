using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

public class AuthorizationTests
{
    [Fact]
    public async Task CreateQuote_WithoutWriteScope_Returns403()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test",
                            _ => { });
                });
            });

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Test Author",
                text = "Test quote"
            });

        Assert.Equal(
            System.Net.HttpStatusCode.Forbidden,
            response.StatusCode);
    }
    [Fact]
    public async Task DeleteQuoteOwnedByAnotherUser_Returns403()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            TestAuthHandler>(
                            "Test",
                            _ => { });
                });
            });

        // Create a quote owned by User 2.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<QuoteDbContext>();

            var quote = new Quote
            {
                Author = "User 2",
                Text = "This belongs to User 2",
                UserId = 2
            };

            db.Quotes.Add(quote);
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // TestAuthHandler authenticates us as User 1.
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var response = await client.DeleteAsync(
            "/api/quotes/1");

        Assert.Equal(
            System.Net.HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}

public class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "test@example.com")
        };

        var identity = new ClaimsIdentity(
            claims,
            "Test");

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            "Test");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}