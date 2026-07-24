using System;
using System.Text;
using PodcastSync.Storage;

namespace PodcastSync.PathTemplate;

/// <summary>
/// Renders a device destination path pattern using the tokens
/// {ShowTitle}, {PublishDate} (with optional :format suffix), and {Title}.
/// Known tokens are sanitized; unknown tokens are left untouched so the
/// surrounding literal text (including path separators) survives intact.
/// </summary>
public sealed class DevicePathRenderer
{
    private const string DefaultDateFormat = "yyyy-MM-dd";

    public string Render(string pattern, string showTitle, DateTime publishDate, string title)
    {
        var builder = new StringBuilder(pattern.Length + 16);
        var i = 0;
        while (i < pattern.Length)
        {
            var open = pattern.IndexOf('{', i);
            if (open < 0)
            {
                builder.Append(pattern, i, pattern.Length - i);
                break;
            }

            builder.Append(pattern, i, open - i);

            var close = pattern.IndexOf('}', open + 1);
            if (close < 0)
            {
                builder.Append(pattern, open, pattern.Length - open);
                break;
            }

            var tokenBody = pattern.Substring(open + 1, close - open - 1);
            builder.Append(ResolveToken(tokenBody, showTitle, publishDate, title));
            i = close + 1;
        }

        return builder.ToString();
    }

    private static string ResolveToken(string tokenBody, string showTitle, DateTime publishDate, string title)
    {
        var colon = tokenBody.IndexOf(':');
        var name = colon < 0 ? tokenBody : tokenBody.Substring(0, colon);
        var format = colon < 0 ? null : tokenBody.Substring(colon + 1);

        return name switch
        {
            "ShowTitle" => FileNameSanitizer.Sanitize(showTitle),
            "Title" => FileNameSanitizer.Sanitize(title),
            "PublishDate" => publishDate.ToString(string.IsNullOrEmpty(format) ? DefaultDateFormat : format),
            _ => "{" + tokenBody + "}",
        };
    }
}
