using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PodcastSync.Domain;

namespace PodcastSync.Opml;

/// <summary>
/// Exports a list of subscriptions to OPML 2.0 XML.
/// </summary>
public static class OpmlExporter
{
    public static string Export(IReadOnlyList<Subscription> subscriptions)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("opml",
                new XAttribute("version", "2.0"),
                new XElement("head"),
                new XElement("body",
                    subscriptions.Select(sub =>
                        new XElement("outline",
                            new XAttribute("text", sub.Title),
                            new XAttribute("type", "rss"),
                            new XAttribute("xmlUrl", sub.FeedUrl)
                        )
                    )
                )
            )
        );

        return doc.Declaration + "\n" + doc;
    }
}
