using QuotesApi.Models;

namespace QuotesApi.Tests;

public class CollectionTests
{
    [Fact]
    public void RejectsNameShorterThan3Characters()
    {
        Assert.Throws<ArgumentException>(
            () => new Collection(1, "AB"));
    }

    [Fact]
    public void RejectsNameLongerThan80Characters()
    {
        var name = new string('A', 81);

        Assert.Throws<ArgumentException>(
            () => new Collection(1, name));
    }

    [Fact]
    public void RejectsDuplicateQuote()
    {
        var collection = new Collection(1, "My Quotes");

        collection.AddItem(10);

        Assert.Throws<InvalidOperationException>(
            () => collection.AddItem(10));
    }

    [Fact]
    public void RejectsMoreThan50Items()
    {
        var collection = new Collection(1, "My Quotes");

        for (var i = 1; i <= 50; i++)
            collection.AddItem(i);

        Assert.Throws<InvalidOperationException>(
            () => collection.AddItem(51));
    }

    [Fact]
    public void RemovesQuote()
    {
        var collection = new Collection(1, "My Quotes");

        collection.AddItem(10);
        collection.RemoveItem(10);

        Assert.Empty(collection.Items);
    }
}