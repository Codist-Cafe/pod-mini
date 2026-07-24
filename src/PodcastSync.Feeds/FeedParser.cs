using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace PodcastSync.Feeds;

/// <summary>
/// Parses RSS 2.0 and Atom podcast feeds into normalized records using
/// <see cref="SyndicationFeed"/>. Items without an audio enclosure are dropped;
/// items without a GUID fall back to their audio URL.
/// </summary>
public sealed class FeedParser
{
    private const string EnclosureRelation = "enclosure";

    public ParsedFeed Parse(string xml)
    {
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,   // forbid external entity resolution for security
        });
        var feed = SyndicationFeed.Load(xmlReader);

        var episodes = new List<FeedEpisode>();
        foreach (var item in feed.Items)
        {
            var enclosure = item.Links.FirstOrDefault(link => link.RelationshipType == EnclosureRelation);
            if (enclosure?.Uri is null)
            {
                continue;
            }

            var audioUrl = enclosure.Uri.ToString();
            var guid = string.IsNullOrEmpty(item.Id) ? audioUrl : item.Id;
            var duration = ReadDuration(item);

            episodes.Add(new FeedEpisode
            {
                Guid = guid,
                Title = item.Title?.Text ?? string.Empty,
                PublishDate = item.PublishDate.UtcDateTime,
                DurationSeconds = duration,
                AudioUrl = audioUrl,
            });
        }

        return new ParsedFeed
        {
            Title = feed.Title?.Text ?? string.Empty,
            Description = feed.Description?.Text,
            Episodes = episodes,
        };
    }

    private static int ReadDuration(SyndicationItem item)
    {
        var values = item.ElementExtensions.ReadElementExtensions<string>(
            DurationParser.ElementName, DurationParser.ElementNamespace);

        return values.Count > 0 ? DurationParser.Parse(values[0]) : 0;
    }
}
