using System.Collections.Generic;
using PodcastSync.Storage;

namespace PodcastSync.Downloads.Tests;

/// <summary>
/// In-memory <see cref="IFileSystem"/> for deterministic download tests.
/// </summary>
internal sealed class InMemoryFileSystem : IFileSystem
{
    public HashSet<string> Directories { get; } = new();
    public Dictionary<string, byte[]> Files { get; } = new();

    private static string Norm(string path) => path.Replace('\\', '/');

    public bool DirectoryExists(string path) => Directories.Contains(Norm(path));
    public void CreateDirectory(string path) => Directories.Add(Norm(path));
    public void DeleteDirectory(string path, bool recursive) => Directories.Remove(Norm(path));
    public bool FileExists(string path) => Files.ContainsKey(Norm(path));
    public void WriteAllBytes(string path, byte[] bytes) => Files[Norm(path)] = bytes;
    public void AppendAllBytes(string path, byte[] bytes)
    {
        var key = Norm(path);
        Files.TryGetValue(key, out var existing);
        var buffer = new byte[(existing?.Length ?? 0) + bytes.Length];
        if (existing != null) System.Array.Copy(existing, 0, buffer, 0, existing.Length);
        System.Array.Copy(bytes, 0, buffer, existing?.Length ?? 0, bytes.Length);
        Files[key] = buffer;
    }
    public long GetFileSize(string path) => Files[Norm(path)].Length;
    public void CopyFile(string sourceFile, string destFile, bool overwrite) => Files[Norm(destFile)] = Files[Norm(sourceFile)];
    public byte[] ReadAllBytes(string path) => Files[Norm(path)];
}

internal sealed class HttpRequestSimulatedException : System.Exception
{
    public HttpRequestSimulatedException(string url) : base($"Simulated HTTP failure for {url}.") { }
}
