using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Storage.Tests;

public class FileNameSanitizerTests
{
    [Fact]
    public void Sanitize_StripsAllInvalidPathCharacters()
    {
        var input = "What? \"No\" <way> | done*";

        var result = FileNameSanitizer.Sanitize(input);

        foreach (var c in FileNameSanitizer.InvalidCharacters)
        {
            Assert.DoesNotContain(c, result);
        }
    }

    [Fact]
    public void Sanitize_ReplacesSpacesWithUnderscores_WhenConfigured()
    {
        var result = FileNameSanitizer.Sanitize("Architecture Patterns in NET 10", replaceSpaces: true);

        Assert.Equal("Architecture_Patterns_in_NET_10", result);
    }

    [Fact]
    public void Sanitize_LeavesSpaces_WhenNotConfigured()
    {
        var result = FileNameSanitizer.Sanitize("Architecture Patterns", replaceSpaces: false);

        Assert.Equal("Architecture Patterns", result);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("com1")]
    [InlineData("LPT9")]
    public void Sanitize_NeutralizesReservedWindowsNames(string reserved)
    {
        var result = FileNameSanitizer.Sanitize(reserved);

        Assert.NotEqual(reserved, result);
        Assert.True(result.Length > reserved.Length);
    }

    [Theory]
    [InlineData("name.", "name")]
    [InlineData("name ", "name")]
    [InlineData(".hidden", ".hidden")] // leading dot kept (not reserved) but no trailing
    public void Sanitize_StripsTrailingDotsAndSpaces(string input, string expected)
    {
        var result = FileNameSanitizer.Sanitize(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Truncate_PreservesExtension_AndStaysUnder240()
    {
        var longName = new string('x', 300) + ".mp3";

        var result = FileNameSanitizer.Sanitize(longName);

        Assert.True(result.Length <= 240);
        Assert.EndsWith(".mp3", result);
    }

    [Fact]
    public void Truncate_WithoutExtension_StaysUnder240()
    {
        var longName = new string('y', 300);

        var result = FileNameSanitizer.Sanitize(longName);

        Assert.True(result.Length <= 240);
    }

    [Fact]
    public void Sanitize_EmptyOrNull_YieldsSafeFallback()
    {
        Assert.Equal("_", FileNameSanitizer.Sanitize(""));
        Assert.Equal("_", FileNameSanitizer.Sanitize("   "));
    }
}
