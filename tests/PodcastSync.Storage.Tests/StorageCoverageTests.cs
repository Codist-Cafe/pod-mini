using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Storage.Tests;

public class StorageCoverageTests
{
    [Fact]
    public void Sanitize_InputOfOnlyInvalidCharacters_YieldsFallback()
    {
        Assert.Equal("_", FileNameSanitizer.Sanitize(":*?<>|"));
    }

    [Fact]
    public void Sanitize_EmptyBaseWithExtension_YieldsFallbackBaseKeepingExtension()
    {
        // base collapses to nothing after stripping trailing dots; extension survives
        var result = FileNameSanitizer.Sanitize("...txt");

        Assert.Equal("_.txt", result);
    }

    [Fact]
    public void LibraryResolver_ExposesLibraryRoot()
    {
        var resolver = new LibraryPathResolver("/podcasts");

        Assert.Equal("/podcasts", resolver.LibraryRoot);
    }
}
