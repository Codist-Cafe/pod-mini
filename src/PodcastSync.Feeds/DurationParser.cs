namespace PodcastSync.Feeds;

/// <summary>
/// Parses an iTunes podcast <c>duration</c> value into seconds.
/// Accepts plain seconds ("3600") and colon forms ("1:02:03", "02:30").
/// Missing, blank, or unparsable values yield 0.
/// </summary>
public static class DurationParser
{
    private const string ItunesDurationNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
    private const string DurationElement = "duration";

    public static readonly string ElementNamespace = ItunesDurationNamespace;
    public static readonly string ElementName = DurationElement;

    public static int Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0;
        }

        var value = raw.Trim();

        if (value.Contains(':'))
        {
            return ParseColonForm(value);
        }

        return int.TryParse(value, out var seconds) ? seconds : 0;
    }

    private static int ParseColonForm(string value)
    {
        var parts = value.Split(':');
        var total = 0;
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var segment))
            {
                return 0;
            }

            // power is the place value of this segment (0 = seconds, 1 = minutes, >=2 = hours)
            var power = parts.Length - 1 - i;
            var multiplier = power switch
            {
                0 => 1,
                1 => 60,
                _ => 3600,
            };
            total += segment * multiplier;
        }

        return total;
    }
}
