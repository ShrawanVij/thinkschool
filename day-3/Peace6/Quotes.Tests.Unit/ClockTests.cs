using FluentAssertions;
using QuotesApi.Services;

namespace QuotesApi.Tests;

public class ClockTests
{
    [Fact]
    public void FakeClock_FixedTime_ReturnsExpectedTime()
    {
        // Arrange
        var expected = new DateTimeOffset(
            2026, 8, 11, 10, 0, 0, TimeSpan.Zero);

        var clock = new FakeClock
        {
            UtcNow = expected
        };

        // Act
        var result = clock.UtcNow;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FakeClock_ZeroTime_ReturnsZeroTime()
    {
        // Arrange
        var expected = DateTimeOffset.MinValue;

        var clock = new FakeClock
        {
            UtcNow = expected
        };

        // Act
        var result = clock.UtcNow;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FakeClock_FutureTime_ReturnsFutureTime()
    {
        // Arrange
        var expected = new DateTimeOffset(
            2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var clock = new FakeClock
        {
            UtcNow = expected
        };

        // Act
        var result = clock.UtcNow;

        // Assert
        result.Should().Be(expected);
    }
}