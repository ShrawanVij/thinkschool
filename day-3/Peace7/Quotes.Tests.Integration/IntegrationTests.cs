using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuotesApi.Data;
using QuotesApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Quotes.Tests.Integration;

public class IntegrationTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public IntegrationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }
    [Fact]
    public async Task GetQuotes_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/quotes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMissingQuote_Returns404()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/quotes/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuotes_InvalidPagination_Returns400()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/quotes?page=0");

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateQuote_WithoutToken_Returns401()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Test",
                text = "Hello"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateQuote_WithWriteScope_Returns201()
    {
        using var factory =
            CreateAuthenticatedFactory("quotes.write");

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Test Author",
                text = "Integration test quote"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateQuote_WithoutWriteScope_Returns403()
    {
        using var factory =
            CreateAuthenticatedFactory();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Test",
                text = "Test"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateQuote_InvalidData_ReturnsProblemDetails()
    {
        using var factory =
            CreateAuthenticatedFactory("quotes.write");

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "",
                text = ""
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        response.Content.Headers.ContentType!
            .MediaType.Should()
            .Be("application/problem+json");
    }

    [Fact]
    public async Task DeleteQuote_WithoutToken_Returns401()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            "/api/quotes/1");

        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteQuote_NotOwned_Returns403()
    {
        using var factory =
            CreateAuthenticatedFactory();

        int otherQuoteId;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<QuoteDbContext>();

            var otherQuote = new Quote
            {
                Author = "Other User",
                Text = "Other quote",
                UserId = 2
            };

            db.Quotes.Add(otherQuote);
            await db.SaveChangesAsync();

            otherQuoteId = otherQuote.Id;
        }

        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/quotes/{otherQuoteId}");

        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCollection_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/collections",
            new
            {
                ownerId = 1,
                name = "My Collection"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddItemToMissingCollection_Returns404()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/collections/999/items",
            new
            {
                quoteId = 1
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteItemFromMissingCollection_Returns404()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            "/collections/999/items/1");

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "wrong@example.com",
                password = "wrong"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "test@example.com",
                password = "Test123!"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                refreshToken = "invalid-token"
            });

        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Root_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetQuotes_ValidPagination_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/quotes?page=1&size=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExistingQuote_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<QuoteDbContext>();

            db.Quotes.Add(new Quote
            {
                Author = "Test Author",
                Text = "Existing quote",
                UserId = 1
            });

            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/quotes/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCollection_ValidRequest_Returns200()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/collections",
            new
            {
                ownerId = 1,
                name = "Integration Collection"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DatabaseMigrations_AreApplied()
    {
        using var factory = new CustomWebApplicationFactory(_fixture);

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<QuoteDbContext>();

        var canConnect =
            await db.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    private WebApplicationFactory<Program>
        CreateAuthenticatedFactory(
            string? scope = null)
    {
        var factory =
            new CustomWebApplicationFactory(_fixture);

        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        "Test";

                    options.DefaultChallengeScheme =
                        "Test";
                })
                .AddScheme<TestAuthOptions, TestAuthHandler>(
                    "Test",
                    options =>
                    {
                        options.Scope = scope;
                    });
            });
        });
    }
}

public sealed class TestAuthOptions
    : AuthenticationSchemeOptions
{
    public string? Scope { get; set; }
}

public class TestAuthHandler
    : AuthenticationHandler<TestAuthOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                "1"),

            new(
                ClaimTypes.Name,
                "test@example.com")
        };

        if (!string.IsNullOrEmpty(Options.Scope))
        {
            claims.Add(
                new Claim("scope", Options.Scope));
        }

        var identity = new ClaimsIdentity(
            claims,
            "Test");

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                "Test");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}