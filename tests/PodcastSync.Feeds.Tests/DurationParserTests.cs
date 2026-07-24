using PodcastSync.Feeds;
using Xunit;

namespace PodcastSync.Feeds.Tests;

public class DurationParserTests
{
    [Theory]
    [InlineData("3600", 3600)]
    [InlineData("0", 0)]
    public void ParsesPlainSeconds(string raw, int expected)
    {
        Assert.Equal(expected, DurationParser.Parse(raw));
    }

    [Theory]
    [InlineData("1:02:03", 3723)]
    [InlineData("02:30", 150)]
    [InlineData("10", 10)]
    public void ParsesColonAndScalarForms(string raw, int expected)
    {
        Assert.Equal(expected, DurationParser.Parse(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingOrBlank_DoesNotThrow_AndYieldsZero(string? raw)
    {
        Assert.Equal(0, DurationParser.Parse(raw));
    }

    [Fact]
    public void UnparsableValue_YieldsZero()
    {
        Assert.Equal(0, DurationParser.Parse("not-a-number"));
    }

    [Fact]
    public void ColonFormWithNonNumericSegment_YieldsZero()
    {
        Assert.Equal(0, DurationParser.Parse("1:abc:30"));
    }
}
