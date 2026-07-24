using System;
using System.Collections.Generic;

namespace PodcastSync.Storage;

/// <summary>
/// Cross-platform (FAT32/exFAT/NTFS/ext4) filename and folder-name sanitizer.
/// Strips invalid path characters, optionally replaces spaces, neutralizes
/// reserved Windows device names, strips trailing dots/spaces, and truncates
/// names longer than 240 characters while preserving the file extension.
/// </summary>
public static class FileNameSanitizer
{
    public static readonly char[] InvalidCharacters = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    private const int MaxLength = 240;
    private const string Fallback = "_";

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    public static string Sanitize(string name, bool replaceSpaces = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Fallback;
        }

        var stripped = RemoveInvalidCharacters(name);

        if (replaceSpaces)
        {
            stripped = stripped.Replace(' ', '_');
        }

        SplitBaseAndExtension(stripped, out var baseName, out var extension);

        baseName = NeutralizeReserved(baseName);
        baseName = baseName.TrimEnd('.', ' ');

        if (string.IsNullOrEmpty(baseName) && string.IsNullOrEmpty(extension))
        {
            return Fallback;
        }

        if (string.IsNullOrEmpty(baseName))
        {
            baseName = Fallback;
        }

        if (string.IsNullOrEmpty(extension))
        {
            return Truncate(baseName, MaxLength);
        }

        return Truncate(baseName, MaxLength - extension.Length) + extension;
    }

    private static string RemoveInvalidCharacters(string name)
    {
        var buffer = new char[name.Length];
        var written = 0;
        foreach (var c in name)
        {
            var invalid = false;
            foreach (var bad in InvalidCharacters)
            {
                if (c == bad)
                {
                    invalid = true;
                    break;
                }
            }

            if (!invalid)
            {
                buffer[written++] = c;
            }
        }

        return new string(buffer, 0, written);
    }

    private static void SplitBaseAndExtension(string name, out string baseName, out string extension)
    {
        var dot = name.LastIndexOf('.');
        if (dot > 0 && dot < name.Length - 1)
        {
            baseName = name.Substring(0, dot);
            extension = name.Substring(dot);
            return;
        }

        baseName = name;
        extension = string.Empty;
    }

    private static string NeutralizeReserved(string baseName)
    {
        return ReservedNames.Contains(baseName) ? baseName + "_" : baseName;
    }

    private static string Truncate(string value, int max)
    {
        return value.Length <= max ? value : value.Substring(0, max);
    }
}
