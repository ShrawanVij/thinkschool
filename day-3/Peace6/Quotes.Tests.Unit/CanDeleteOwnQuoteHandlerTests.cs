using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuotesApi.Authorization;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Tests;

public class CanDeleteOwnQuoteHandlerTests
{
    [Fact]
    public async Task Handle_UserOwnsQuote_Succeeds()
    {
        // Arrange
        await using var connection = new SqliteConnection(
            "DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new QuoteDbContext(options);
        await db.Database.EnsureCreatedAsync();

        db.Quotes.Add(new Quote
        {
            Id = 1,
            Author = "User 1",
            Text = "My quote",
            UserId = 1
        });

        await db.SaveChangesAsync();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "1")
                },
                "Test"));

        var requirement = new CanDeleteOwnQuoteRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            1);

        var handler = new CanDeleteOwnQuoteHandler(db);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnQuote_DoesNotSucceed()
    {
        // Arrange
        await using var connection = new SqliteConnection(
            "DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new QuoteDbContext(options);
        await db.Database.EnsureCreatedAsync();

        db.Quotes.Add(new Quote
        {
            Id = 1,
            Author = "User 2",
            Text = "Someone else's quote",
            UserId = 2
        });

        await db.SaveChangesAsync();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "1")
                },
                "Test"));

        var requirement = new CanDeleteOwnQuoteRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            1);

        var handler = new CanDeleteOwnQuoteHandler(db);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UserHasNoNameIdentifier_DoesNotSucceed()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new QuoteDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity("Test"));

        var requirement = new CanDeleteOwnQuoteRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            1);

        var handler = new CanDeleteOwnQuoteHandler(db);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidUserIdClaim_DoesNotSucceed()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new QuoteDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "abc")
                },
                "Test"));

        var requirement = new CanDeleteOwnQuoteRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            1);

        var handler = new CanDeleteOwnQuoteHandler(db);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_QuoteDoesNotExist_DoesNotSucceed()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new QuoteDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1")
                },
                "Test"));

        var requirement = new CanDeleteOwnQuoteRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            999);

        var handler = new CanDeleteOwnQuoteHandler(db);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}