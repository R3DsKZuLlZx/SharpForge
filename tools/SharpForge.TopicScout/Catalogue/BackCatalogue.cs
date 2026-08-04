using System.Globalization;
using System.Text.RegularExpressions;

namespace SharpForge.TopicScout.Catalogue;

/// <summary>A published post, as read from Content/Blog/*.md.</summary>
public sealed record CataloguePost(
    string Slug,
    string Title,
    string Category,
    DateTimeOffset? Date,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Headings)
{
    /// <summary>Every term that should make a feed item "match" this post.</summary>
    public IEnumerable<string> IndexTerms =>
        Slug.Split('-')
            .Concat(Tokeniser.Tokenise(Title))
            .Concat(Tags.SelectMany(Tokeniser.Tokenise))
            .Concat(Headings.SelectMany(Tokeniser.Tokenise));
}

/// <summary>
/// Reads the published blog posts so candidates can be scored against what
/// already exists. This is the relevance model: a topic matters to *this* blog
/// if it overlaps a post already owned.
/// </summary>
public static partial class BackCatalogue
{
    public static IReadOnlyList<CataloguePost> Load(string repositoryRoot)
    {
        var dir = Path.Combine(repositoryRoot, "src", "SharpForge.Client", "Content", "Blog");
        if (!Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException($"Blog content directory not found: {dir}");
        }

        var posts = new List<CataloguePost>();

        foreach (var file in Directory.EnumerateFiles(dir, "*.md"))
        {
            var text = File.ReadAllText(file);
            var slug = Path.GetFileNameWithoutExtension(file);

            posts.Add(new CataloguePost(
                slug,
                Scalar(text, "title") ?? slug,
                Scalar(text, "category") ?? "",
                ParsePostDate(Scalar(text, "date")),
                ParseTags(text),
                HeadingRegex().Matches(text).Select(m => m.Groups[1].Value.Trim()).ToList()));
        }

        return posts;
    }

    private static DateTimeOffset? ParsePostDate(string? value) =>
        value is not null && DateTimeOffset.TryParseExact(
            value, "MMMM d, yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var d)
            ? d
            : null;

    private static string? Scalar(string text, string key)
    {
        var m = Regex.Match(text, $@"^{key}:\s*""?(?<v>[^""\r\n]+)""?\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return m.Success ? m.Groups["v"].Value.Trim() : null;
    }

    private static List<string> ParseTags(string text)
    {
        var m = Regex.Match(text, @"^tags:\s*\[(?<v>[^\]]*)\]", RegexOptions.Multiline);
        if (!m.Success)
        {
            return [];
        }

        return m.Groups["v"].Value
            .Split(',')
            .Select(t => t.Trim().Trim('"'))
            .Where(t => t.Length > 0)
            .ToList();
    }

    [GeneratedRegex(@"^##\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex HeadingRegex();
}

/// <summary>Shared tokenisation so candidates and posts are compared consistently.</summary>
public static partial class Tokeniser
{
    /// <summary>
    /// Words carrying no signal in a .NET feed. ".NET", "C#" and similar are
    /// deliberately included — they appear in nearly every headline and would
    /// otherwise match everything.
    /// </summary>
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the","a","an","and","or","but","for","with","without","from","into","your","you",
        "is","are","was","were","be","being","been","to","of","in","on","at","by","it","its",
        "this","that","these","those","how","what","why","when","which","who","new","using",
        "use","guide","introduction","intro","part","tips","best","practices","deep","dive",
        "net","dotnet","csharp","c","microsoft","announcing","announcement","available",
        "release","releases","released","update","updates","preview","now","more","complete",
        "understanding","exploring","building","build","create","creating","get","getting",
        "started","overview","first","look","things","should","know","vs","versus","než"
    };

    public static IEnumerable<string> Tokenise(string text)
    {
        foreach (Match m in WordRegex().Matches(text))
        {
            var raw = m.Value.ToLowerInvariant().Trim('.', '-', '_');
            if (raw.Length == 0)
            {
                continue;
            }

            foreach (var part in Split(raw))
            {
                if (part.Length < 3 || Stopwords.Contains(part))
                {
                    continue;
                }

                var stem = Stem(part);
                if (stem.Length >= 3 && !Stopwords.Contains(stem))
                {
                    yield return stem;
                }
            }
        }
    }

    /// <summary>
    /// Yields the whole token plus its hyphen-separated parts, so "unit-test"
    /// can match a post tagged "Unit Testing".
    /// </summary>
    private static IEnumerable<string> Split(string raw)
    {
        yield return raw;

        if (!raw.Contains('-'))
        {
            yield break;
        }

        foreach (var part in raw.Split('-', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return part;
        }
    }

    /// <summary>
    /// Deliberately crude suffix stripping — enough to unify testing/test and
    /// migrations/migration without pulling in a real stemmer.
    /// </summary>
    private static string Stem(string token) => token switch
    {
        { Length: > 5 } when token.EndsWith("ing", StringComparison.Ordinal) => token[..^3],
        { Length: > 4 } when token.EndsWith("ies", StringComparison.Ordinal) => token[..^3] + "y",
        { Length: > 4 } when token.EndsWith("es", StringComparison.Ordinal) => token[..^2],
        { Length: > 3 } when token.EndsWith("s", StringComparison.Ordinal)
                            && !token.EndsWith("ss", StringComparison.Ordinal) => token[..^1],
        _ => token
    };

    [GeneratedRegex(@"[A-Za-z][A-Za-z0-9\.\-_#\+]*")]
    private static partial Regex WordRegex();
}

