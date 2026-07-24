using System.IO;
using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Storage.Tests;

public class LibraryPathResolverTests
{
    [Fact]
    public void ResolveLocalFilePath_JoinsSanitizedSegmentsWithSeparator()
    {
        var resolver = new LibraryPathResolver("/home/user/Podcasts");

        var path = resolver.ResolveLocalFilePath("Software Engineering Daily", "2026-07-24_Architecture.mp3");

        var expected = Path.Join("/home/user/Podcasts", "Software Engineering Daily", "2026-07-24_Architecture.mp3");
        Assert.Equal(expected, path);
    }

    [Fact]
    public void ResolveLocalFilePath_SanitizesShowTitleAndFileName()
    {
        var resolver = new LibraryPathResolver("/home/user/Podcasts");

        var path = resolver.ResolveLocalFilePath("Show: Bad?", "ep|name.mp3");

        Assert.DoesNotContain(':', path);
        Assert.DoesNotContain('?', path);
        Assert.DoesNotContain('|', path);
    }

    [Fact]
    public void FolderNameFor_GeneratesSanitizedFolderNameFromTitle()
    {
        var resolver = new LibraryPathResolver("/home/user/Podcasts");

        var folder = resolver.FolderNameFor("What? \"No\"");

        foreach (var c in FileNameSanitizer.InvalidCharacters)
        {
            Assert.DoesNotContain(c, folder);
        }
        Assert.NotEmpty(folder);
    }
}
