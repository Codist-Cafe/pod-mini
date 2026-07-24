using System.IO;

namespace PodcastSync.Storage;

/// <summary>
/// Production <see cref="IFileSystem"/> backed by the real BCL filesystem.
/// </summary>
public sealed class SystemFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

    public bool FileExists(string path) => File.Exists(path);

    public void WriteAllBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);

    public void AppendAllBytes(string path, byte[] bytes)
    {
        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        stream.Write(bytes, 0, bytes.Length);
    }

    public long GetFileSize(string path) => new FileInfo(path).Length;

    public void CopyFile(string sourceFile, string destFile, bool overwrite) =>
        File.Copy(sourceFile, destFile, overwrite);

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
}
