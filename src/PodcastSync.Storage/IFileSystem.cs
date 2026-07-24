namespace PodcastSync.Storage;

/// <summary>
/// Side-effect-free filesystem abstraction so all IO-adjacent code can reach
/// 100% line coverage with an injected fake. The production implementation wraps
/// the real BCL types.
/// </summary>
public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    void DeleteDirectory(string path, bool recursive);
    bool FileExists(string path);
    void WriteAllBytes(string path, byte[] bytes);
    void AppendAllBytes(string path, byte[] bytes);
    long GetFileSize(string path);
    void CopyFile(string sourceFile, string destFile, bool overwrite);
    byte[] ReadAllBytes(string path);
}
