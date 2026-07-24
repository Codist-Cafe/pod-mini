using System.IO;
using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Storage.Tests;

public class SystemFileSystemTests
{
    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "podcastsync-test-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void Directory_Create_Exists_Delete_RoundTrip()
    {
        var fs = new SystemFileSystem();
        var root = NewTempDir();
        try
        {
            var child = Path.Combine(root, "child");

            Assert.False(fs.DirectoryExists(child));
            fs.CreateDirectory(child);
            Assert.True(fs.DirectoryExists(child));

            fs.DeleteDirectory(child, recursive: true);
            Assert.False(fs.DirectoryExists(child));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void File_Write_Read_Size_Copy_Append_RoundTrip()
    {
        var fs = new SystemFileSystem();
        var root = NewTempDir();
        try
        {
            var file = Path.Combine(root, "a.bin");
            var copy = Path.Combine(root, "b.bin");

            Assert.False(fs.FileExists(file));
            fs.WriteAllBytes(file, new byte[] { 1, 2, 3 });
            Assert.True(fs.FileExists(file));
            Assert.Equal(3, fs.GetFileSize(file));
            Assert.Equal(new byte[] { 1, 2, 3 }, fs.ReadAllBytes(file));

            fs.CopyFile(file, copy, overwrite: true);
            Assert.Equal(new byte[] { 1, 2, 3 }, fs.ReadAllBytes(copy));

            fs.AppendAllBytes(file, new byte[] { 4, 5 });
            Assert.Equal(5, fs.GetFileSize(file));
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fs.ReadAllBytes(file));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void AppendAllBytes_CreatesFile_WhenAbsent()
    {
        var fs = new SystemFileSystem();
        var root = NewTempDir();
        try
        {
            var file = Path.Combine(root, "new.bin");

            fs.AppendAllBytes(file, new byte[] { 9 });

            Assert.True(fs.FileExists(file));
            Assert.Equal(new byte[] { 9 }, fs.ReadAllBytes(file));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
