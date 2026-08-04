using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SharpForge.TopicScout.Model;

namespace SharpForge.TopicScout.Sources;

/// <summary>
/// A pluggable origin of blog topic candidates.
/// Implementations may be deterministic (feeds, APIs) or agentic — the
/// pipeline treats them identically.
/// </summary>
public interface ITopicSource
{
    string Name { get; }

    Task<SourceResult> FetchAsync(HttpClient http, CancellationToken ct);
}

/// <summary>
/// Shared helpers for reading real-world RSS, which is frequently malformed.
/// Both feeds currently wired up break strict parsers in different ways, so
/// nothing here assumes well-formed XML.
/// </summary>
public static partial class FeedReader
{
    /// <summary>
    /// Downloads as bytes and decodes UTF-8 explicitly. Reading via the
    /// response's declared charset produces mojibake on these feeds
    /// (e.g. "What€™s New" instead of "What's New").
    /// </summary>
    public static async Task<string> DownloadTextAsync(HttpClient http, string url, CancellationToken ct)
    {
        var bytes = await http.GetByteArrayAsync(url, ct);
        var text = Encoding.UTF8.GetString(bytes);

        // Leading whitespace/BOM before <?xml is fatal to XmlDocument/XDocument.
        return text.TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
    }

    /// <summary>Strips tags and decodes entities from a fragment of feed HTML.</summary>
    public static string CleanText(string html)
    {
        var text = TagRegex().Replace(html, " ");
        text = WebUtility.HtmlDecode(text);
        return WhitespaceRegex().Replace(text, " ").Trim();
    }

    /// <summary>
    /// Parses the assorted date formats these feeds use. Morning Brew emits
    /// RFC-822; the daily rundown emits bare "yyyy-MM-dd".
    /// </summary>
    public static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Converts the rundown's relative "N days ago" / "N hours ago" text into
    /// an absolute timestamp.
    /// </summary>
    public static DateTimeOffset? ParseRelativeAge(string? text, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var m = RelativeAgeRegex().Match(text);
        if (!m.Success || !int.TryParse(m.Groups["n"].Value, out var n))
        {
            return null;
        }

        return m.Groups["unit"].Value.ToLowerInvariant() switch
        {
            "minute" or "minutes" => now.AddMinutes(-n),
            "hour" or "hours" => now.AddHours(-n),
            "day" or "days" => now.AddDays(-n),
            "week" or "weeks" => now.AddDays(-7 * n),
            "month" or "months" => now.AddMonths(-n),
            _ => null
        };
    }

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"(?<n>\d+)\s*(?<unit>minutes?|hours?|days?|weeks?|months?)\s*ago",
        RegexOptions.IgnoreCase)]
    private static partial Regex RelativeAgeRegex();
}

