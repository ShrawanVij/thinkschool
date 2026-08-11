using FluentAssertions;
using QuotesApi.Models;
using QuotesApi.Services;

namespace Tests.Domain;

public class CollectionTests
{
    private readonly IClock _clock = new FakeClock();

    [Fact]
    public void EmptyName_Throws()
    {
        var act = () => new Collection(1, "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NameOver80Characters_Throws()
    {
        var name = new string('A', 81);

        var act = () => new Collection(1, name);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Adding51stItem_Throws()
    {
        var collection = new Collection(1, "My Quotes");

        for (var i = 1; i <= 50; i++)
            collection.AddItem(i, _clock);

        var act = () => collection.AddItem(51, _clock);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DuplicateQuoteId_Throws()
    {
        var collection = new Collection(1, "My Quotes");

        collection.AddItem(10, _clock);

        var act = () => collection.AddItem(10, _clock);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemovingNonExistentItem_Throws()
    {
        var collection = new Collection(1, "My Quotes");

        var act = () => collection.RemoveItem(10);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AddingThenRemoving_LeavesZeroItems()
    {
        var collection = new Collection(1, "My Quotes");

        collection.AddItem(10, _clock);
        collection.RemoveItem(10);

        collection.Items.Should().BeEmpty();
    }
}

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } =
        new DateTimeOffset(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);
}