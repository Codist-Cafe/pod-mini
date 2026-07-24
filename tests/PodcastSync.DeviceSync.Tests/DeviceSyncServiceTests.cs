using System.Collections.Generic;
using System.IO;
using PodcastSync.DeviceSync;
using PodcastSync.PathTemplate;
using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.DeviceSync.Tests;

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

internal sealed class FakeVolumeInfo : IVolumeInfo
{
    private readonly long _freeSpace;
    public FakeVolumeInfo(long freeSpace) => _freeSpace = freeSpace;
    public long GetAvailableFreeSpace(string path) => _freeSpace;
}

internal static class ItemFactory
{
    public static DeviceTransferItem Ep(string show, System.DateTime date, string title, string source, int size) =>
        new() { ShowTitle = show, PublishDate = date, Title = title, SourceFilePath = source, SizeBytes = size };

    public static DeviceSyncService Service(IFileSystem fs, long freeSpace) =>
        new(new FakeVolumeInfo(freeSpace), fs, new DevicePathRenderer());
}

public class DeviceSyncServiceTests
{
    private static readonly System.DateTime Date = new(2026, 7, 24);

    [Fact]
    public void SendSelected_CopiesChosenEpisodes_ToRenderedDestinations()
    {
        var fs = new InMemoryFileSystem();
        fs.WriteAllBytes("/lib/ep1.mp3", new byte[] { 1, 2, 3, 4 });
        fs.WriteAllBytes("/lib/ep2.mp3", new byte[] { 5, 6, 7, 8 });
        var svc = ItemFactory.Service(fs, freeSpace: 1_000_000);

        var result = svc.TransferAsync(
            new[] { ItemFactory.Ep("SED", Date, "Ep1", "/lib/ep1.mp3", 4), ItemFactory.Ep("SED", Date, "Ep2", "/lib/ep2.mp3", 4) },
            "/dev",
            "{ShowTitle}/{PublishDate:yyyy-MM-dd}_{Title}.mp3");

        Assert.Equal(2, result.Copied);
        Assert.Equal(8, result.BytesCopied);
        Assert.True(fs.FileExists("/dev/SED/2026-07-24_Ep1.mp3"));
        Assert.True(fs.FileExists("/dev/SED/2026-07-24_Ep2.mp3"));
        Assert.True(fs.DirectoryExists("/dev/SED"));
    }

    [Fact]
    public void SubscriptionSync_TransfersOnlyTheDelta()
    {
        var fs = new InMemoryFileSystem();
        fs.WriteAllBytes("/lib/a.mp3", new byte[] { 1 });
        fs.WriteAllBytes("/lib/b.mp3", new byte[] { 2 });
        fs.WriteAllBytes("/lib/c.mp3", new byte[] { 3 });
        // 'a' already on the device with same name+size -> should be skipped
        fs.Directories.Add("/dev/Show");
        fs.WriteAllBytes("/dev/Show/a.mp3", new byte[] { 1 });
        var svc = ItemFactory.Service(fs, freeSpace: 1_000_000);

        var result = svc.TransferAsync(
            new[] {
                ItemFactory.Ep("Show", Date, "a", "/lib/a.mp3", 1),
                ItemFactory.Ep("Show", Date, "b", "/lib/b.mp3", 1),
                ItemFactory.Ep("Show", Date, "c", "/lib/c.mp3", 1),
            },
            "/dev",
            "{ShowTitle}/{Title}.mp3");

        Assert.Equal(2, result.Copied);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public void DuplicateDetection_SkipsSameNameAndSize()
    {
        var fs = new InMemoryFileSystem();
        fs.WriteAllBytes("/lib/x.mp3", new byte[] { 9, 9 });
        fs.Directories.Add("/dev/S");
        fs.WriteAllBytes("/dev/S/x.mp3", new byte[] { 9, 9 });
        var svc = ItemFactory.Service(fs, freeSpace: 1_000_000);

        var result = svc.TransferAsync(
            new[] { ItemFactory.Ep("S", Date, "x", "/lib/x.mp3", 2) },
            "/dev",
            "{ShowTitle}/{Title}.mp3");

        Assert.Equal(0, result.Copied);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public void InsufficientSpace_RefusesTransferAndCopiesNothing()
    {
        var fs = new InMemoryFileSystem();
        fs.WriteAllBytes("/lib/big.mp3", new byte[1000]);
        var svc = ItemFactory.Service(fs, freeSpace: 100);

        var ex = Assert.Throws<InsufficientDeviceSpaceException>(() =>
            svc.TransferAsync(new[] { ItemFactory.Ep("S", Date, "big", "/lib/big.mp3", 1000) }, "/dev", "{ShowTitle}/{Title}.mp3"));

        Assert.Equal(0, fs.Files.Count - 1); // only the source exists, nothing copied
        Assert.True(ex.BytesNeeded > ex.BytesAvailable);
    }

    [Fact]
    public void SufficientSpace_ProceedsWithTransfer()
    {
        var fs = new InMemoryFileSystem();
        fs.WriteAllBytes("/lib/ok.mp3", new byte[10]);
        var svc = ItemFactory.Service(fs, freeSpace: 1_000_000);

        var result = svc.TransferAsync(new[] { ItemFactory.Ep("S", Date, "ok", "/lib/ok.mp3", 10) }, "/dev", "{ShowTitle}/{Title}.mp3");

        Assert.Equal(1, result.Copied);
        Assert.True(fs.FileExists("/dev/S/ok.mp3"));
    }
}
