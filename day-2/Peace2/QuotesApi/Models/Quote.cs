namespace QuotesApi.Models;

public class Quote
{
    public int Id { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }

    private Quote()
    {
    }

    private Quote(string author, string text)
    {
        Author = author;
        Text = text;
    }

    public static (Quote? Quote, DomainError? Error) Create(
        string author,
        string text)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return (
                null,
                new DomainError(
                    "AuthorRequired",
                    "Author is required."));
        }

        if (author.Length > 200)
        {
            return (
                null,
                new DomainError(
                    "AuthorTooLong",
                    "Author cannot exceed 200 characters."));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return (
                null,
                new DomainError(
                    "TextRequired",
                    "Text is required."));
        }

        if (text.Length > 1000)
        {
            return (
                null,
                new DomainError(
                    "TextTooLong",
                    "Text cannot exceed 1000 characters."));
        }

        return (new Quote(author, text), null);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}