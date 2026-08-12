using FluentAssertions;
using QuotesApi.Services;

namespace QuotesApi.Tests;

public class QuoteFormatterTests
{
    [Fact]
    public void Format_TextWithSpaces_ReturnsTrimmedText()
    {
        // Arrange
        var formatter = new QuoteFormatter();
        var text = "  Hello world  ";

        // Act
        var result = formatter.Format(text);

        // Assert
        result.Should().Be("Hello world");
    }

    [Fact]
    public void Format_TextWithoutSpaces_ReturnsSameText()
    {
        // Arrange
        var formatter = new QuoteFormatter();
        var text = "Hello world";

        // Act
        var result = formatter.Format(text);

        // Assert
        result.Should().Be("Hello world");
    }

    [Fact]
    public void Format_OnlySpaces_ReturnsEmptyString()
    {
        // Arrange
        var formatter = new QuoteFormatter();
        var text = "     ";

        // Act
        var result = formatter.Format(text);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("  Hello  ", "Hello")]
    [InlineData(" Hello", "Hello")]
    [InlineData("Hello ", "Hello")]
    [InlineData("  Hello World  ", "Hello World")]
    [InlineData("Test", "Test")]
    [InlineData("   ", "")]
    public void Format_VariousInputs_ReturnsExpectedResult(
        string input,
        string expected)
    {
        // Arrange
        var formatter = new QuoteFormatter();

        // Act
        var result = formatter.Format(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\tHello\t", "Hello")]
    [InlineData("\nHello\n", "Hello")]
    [InlineData("  Hello World  ", "Hello World")]
    public void Format_WhitespaceVariants_ReturnsTrimmedText(
        string input,
        string expected)
    {
        // Arrange
        var formatter = new QuoteFormatter();

        // Act
        var result = formatter.Format(input);

        // Assert
        result.Should().Be(expected);
    }
}