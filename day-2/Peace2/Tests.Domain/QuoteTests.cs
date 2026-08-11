using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class QuoteTests
{
    [Fact]
    public void EmptyAuthor_ReturnsDomainError()
    {
        var result = Quote.Create("", "A valid quote");

        result.Error.Should().NotBeNull();
        result.Quote.Should().BeNull();
    }

    [Fact]
    public void AuthorOver200Characters_ReturnsDomainError()
    {
        var author = new string('A', 201);

        var result = Quote.Create(author, "A valid quote");

        result.Error.Should().NotBeNull();
        result.Quote.Should().BeNull();
    }

    [Fact]
    public void EmptyText_ReturnsDomainError()
    {
        var result = Quote.Create("Author", "");

        result.Error.Should().NotBeNull();
        result.Quote.Should().BeNull();
    }

    [Fact]
    public void TextOver1000Characters_ReturnsDomainError()
    {
        var text = new string('A', 1001);

        var result = Quote.Create("Author", text);

        result.Error.Should().NotBeNull();
        result.Quote.Should().BeNull();
    }

    [Fact]
    public void ValidQuote_ReturnsQuote()
    {
        var result = Quote.Create("Author", "A valid quote");

        result.Error.Should().BeNull();
        result.Quote.Should().NotBeNull();
        result.Quote!.Author.Should().Be("Author");
        result.Quote.Text.Should().Be("A valid quote");
    }

    [Fact]
    public void SoftDelete_SetsIsDeleted()
    {
        var result = Quote.Create("Author", "A valid quote");
        var quote = result.Quote!;

        quote.SoftDelete();

        quote.IsDeleted.Should().BeTrue();
    }
}