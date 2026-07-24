using System.IO;

namespace PodcastSync.Storage;

/// <summary>
/// Resolves on-disk library paths. Each episode lives at
/// {LibraryRoot}/{SanitizedShowTitle}/{SanitizedFileName}.
/// </summary>
public sealed class LibraryPathResolver
{
    private readonly string _libraryRoot;

    public LibraryPathResolver(string libraryRoot)
    {
        _libraryRoot = libraryRoot;
    }

    public string LibraryRoot => _libraryRoot;

    public string FolderNameFor(string showTitle)
    {
        return FileNameSanitizer.Sanitize(showTitle);
    }

    public string ResolveLocalFilePath(string showTitle, string fileName)
    {
        var folder = FileNameSanitizer.Sanitize(showTitle);
        var file = FileNameSanitizer.Sanitize(fileName);
        return Path.Join(_libraryRoot, folder, file);
    }
}
