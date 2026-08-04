using System.Text.RegularExpressions;
using SharpForge.TopicScout.Model;

namespace SharpForge.TopicScout.Sources;

/// <summary>
/// feeds.thechels.uk daily .NET rundown.
///
/// Quirks handled here:
/// - Content is NOT one RSS item per link. There is one item per day whose
///   CDATA body contains an HTML &lt;ul&gt; of ~30 links.
/// - Each link's date is a relative string ("6 days ago"), not a timestamp.
/// - The document has leading whitespace before the XML declaration and a
///   malformed &lt;link&gt;&gt; element, so it is parsed with regex rather than
///   an XML reader.
/// </summary>
public sealed partial class DailyRundownSource : ITopicSource
{
    private const string FeedUrl = "https://feeds.thechels.uk/dotnet.rss";

    public string Name => "thechels .NET rundown";

    public async Task<SourceResult> FetchAsync(HttpClient http, CancellationToken ct)
    {
        try
        {
            var raw = await FeedReader.DownloadTextAsync(http, FeedUrl, ct);
            var now = DateTimeOffset.UtcNow;

            var candidates = new List<TopicCandidate>();

            foreach (Match m in ListItemRegex().Matches(raw))
            {
                var title = FeedReader.CleanText(m.Groups["title"].Value);
                var url = m.Groups["url"].Value.Trim();
                var published = FeedReader.ParseRelativeAge(m.Groups["age"].Value, now);

                if (title.Length == 0 || url.Length == 0)
                {
                    continue;
                }

                candidates.Add(new TopicCandidate(title, url, Name, published));
            }

            // Deduplicate: the same link recurs across daily rundowns.
            var deduped = candidates
                .GroupBy(c => c.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(c => c.Published ?? DateTimeOffset.MinValue).First())
                .ToList();

            var newest = deduped
                .Where(c => c.Published is not null)
                .Select(c => c.Published!.Value)
                .DefaultIfEmpty()
                .Max();

            return new SourceResult(Name, deduped,
                newest == default ? null : newest, null);
        }
        catch (Exception ex)
        {
            return new SourceResult(Name, [], null, ex.Message);
        }
    }

    /// <summary>Matches: &lt;li&gt;Title (&lt;a href="url"&gt;6 days ago&lt;/a&gt;)&lt;/li&gt;</summary>
    [GeneratedRegex(
        """<li>(?<title>.*?)\s*\(<a href="(?<url>[^"]+)">(?<age>[^<]*)</a>\)</li>""",
        RegexOptions.Singleline)]
    private static partial Regex ListItemRegex();
}

