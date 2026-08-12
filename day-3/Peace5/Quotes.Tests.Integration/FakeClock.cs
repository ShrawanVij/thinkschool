using QuotesApi.Services;

namespace Quotes.Tests.Integration;

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}