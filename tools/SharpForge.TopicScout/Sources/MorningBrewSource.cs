using System.Text.RegularExpressions;
using System.Xml.Linq;
using SharpForge.TopicScout.Model;

namespace SharpForge.TopicScout.Sources;

/// <summary>
/// Chris Alcock's "The Morning Brew" — a human-curated daily .NET link roundup.
///
/// ⚠️ As of writing this feed appears dormant: the newest issue is #3995 dated
/// 2 August 2024, whose body announces a summer break it never returned from.
/// It is wired up anyway because it costs nothing and will resume working
/// automatically if publication restarts. The pipeline's staleness check
/// (<see cref="SourceResult.IsStale"/>) is what stops that being invisible.
///
/// Structure: standard RSS items, one per daily issue, with the links inside
/// content:encoded rather than as individual items.
/// </summary>
public sealed partial class MorningBrewSource : ITopicSource
{
    private const string FeedUrl = "https://blog.cwa.me.uk/feed/";

    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";

    public string Name => "The Morning Brew";

    public async Task<SourceResult> FetchAsync(HttpClient http, CancellationToken ct)
    {
        try
        {
            var raw = await FeedReader.DownloadTextAsync(http, FeedUrl, ct);
            var doc = XDocument.Parse(raw);

            var candidates = new List<TopicCandidate>();
            DateTimeOffset? newest = null;

            foreach (var item in doc.Descendants("item"))
            {
                var published = FeedReader.ParseDate(item.Element("pubDate")?.Value);
                if (published is not null && (newest is null || published > newest))
                {
                    newest = published;
                }

                var body = item.Element(ContentNs + "encoded")?.Value
                           ?? item.Element("description")?.Value
                           ?? "";

                foreach (Match m in AnchorRegex().Matches(body))
                {
                    var url = m.Groups["url"].Value.Trim();
                    var title = FeedReader.CleanText(m.Groups["text"].Value);

                    if (title.Length < 15 || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // skip nav links, "read more", author links
                    }

                    if (IsSelfLink(url))
                    {
                        continue;
                    }

                    candidates.Add(new TopicCandidate(title, url, Name, published));
                }
            }

            var deduped = candidates
                .GroupBy(c => c.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            return new SourceResult(Name, deduped, newest, null);
        }
        catch (Exception ex)
        {
            return new SourceResult(Name, [], null, ex.Message);
        }
    }

    private static bool IsSelfLink(string url) =>
        url.Contains("cwa.me.uk", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("""<a[^>]+href="(?<url>[^"]+)"[^>]*>(?<text>.*?)</a>""",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex AnchorRegex();
}

