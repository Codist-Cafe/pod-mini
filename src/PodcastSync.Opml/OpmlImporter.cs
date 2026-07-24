using System.Collections.Generic;
using System.Xml.Linq;

namespace PodcastSync.Opml;

/// <summary>
/// Extracts subscription feed URLs from an OPML document.
/// Only <c>outline</c> elements carrying an <c>xmlUrl</c> attribute are returned;
/// folder-only outlines and other metadata are ignored.
/// </summary>
public static class OpmlImporter
{
    public static IReadOnlyList<string> ImportFeedUrls(string opmlXml)
    {
        var doc = XDocument.Parse(opmlXml);
        return doc
            .Descendants("outline")
            .Select(el => el.Attribute("xmlUrl")?.Value)
            .Where(url => url is not null)
            .Cast<string>()
            .ToList();
    }
}
