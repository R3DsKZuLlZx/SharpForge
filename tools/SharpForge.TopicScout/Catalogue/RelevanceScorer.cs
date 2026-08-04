using System.Text.RegularExpressions;
using SharpForge.TopicScout.Model;

namespace SharpForge.TopicScout.Catalogue;

/// <summary>
/// Scores feed candidates against the back catalogue.
///
/// The deterministic layer deliberately does the cheap, reliable work — noise
/// filtering, deduplication, gap/decay classification — leaving only a short
/// list for a human or an agent to judge. That is where the judgement is
/// actually reliable.
/// </summary>
public sealed class RelevanceScorer(IReadOnlyList<CataloguePost> catalogue, IReadOnlyList<string> knownTopics)
{
    /// <summary>Minimum shared terms before a candidate counts as matching a post.</summary>
    private const int MatchThreshold = 2;

    private readonly List<(CataloguePost Post, HashSet<string> Terms)> _index = catalogue
        .Select(p => (p, p.IndexTerms.ToHashSet(StringComparer.OrdinalIgnoreCase)))
        .ToList();

    public ScoredCandidate Score(TopicCandidate candidate)
    {
        if (NoiseFilter.IsNoise(candidate, out var noiseReason))
        {
            return new ScoredCandidate(candidate, CandidateKind.Noise, 0, null, null, [], noiseReason);
        }

        var tokens = Tokeniser.Tokenise(candidate.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (IsAlreadyQueued(candidate.Title, tokens, out var dupeOf))
        {
            return new ScoredCandidate(candidate, CandidateKind.Duplicate, 0, null, null, [],
                $"already in the topic queue: {dupeOf}");
        }

        var best = _index
            .Select(e => (e.Post, Shared: e.Terms.Intersect(tokens, StringComparer.OrdinalIgnoreCase).ToList()))
            .Where(x => x.Shared.Count >= MatchThreshold)
            .OrderByDescending(x => x.Shared.Count)
            .FirstOrDefault();

        if (best.Post is not null)
        {
            // Overlaps an existing post, and the news is newer than the post →
            // that post may now be teaching something out of date.
            var score = best.Shared.Count
                        + RecencyBonus(candidate)
                        + StalenessBonus(best.Post)
                        - VendorPenalty(candidate);

            return new ScoredCandidate(candidate, CandidateKind.Decay, score,
                best.Post.Slug, best.Post.Date, best.Shared,
                $"overlaps '{best.Post.Slug}' on: {string.Join(", ", best.Shared.Take(4))}");
        }

        return new ScoredCandidate(candidate, CandidateKind.Gap,
            1 + RecencyBonus(candidate) - VendorPenalty(candidate),
            null, null, tokens.Take(5).ToList(),
            "no existing post covers this");
    }

    /// <summary>
    /// Vendor blogs mix genuine technique with product marketing. Rather than
    /// enumerate every marketing headline, demote them so they surface only
    /// when the topical match is strong.
    /// </summary>
    private static double VendorPenalty(TopicCandidate c) =>
        NoiseFilter.IsVendorHost(c.Host) ? 1.5 : 0;

    /// <summary>Fresher news is more actionable — a week-old release is a better peg than a year-old one.</summary>
    private static double RecencyBonus(TopicCandidate c) => c.AgeDays switch
    {
        null => 0,
        <= 3 => 3,
        <= 7 => 2,
        <= 21 => 1,
        _ => 0
    };

    /// <summary>An older post that just got contradicted is a more urgent fix.</summary>
    private static double StalenessBonus(CataloguePost post)
    {
        if (post.Date is null)
        {
            return 0;
        }

        var days = (DateTimeOffset.UtcNow - post.Date.Value).TotalDays;
        return days switch
        {
            > 365 => 2,
            > 180 => 1,
            _ => 0
        };
    }

    private bool IsAlreadyQueued(string title, HashSet<string> tokens, out string? match)
    {
        foreach (var known in knownTopics)
        {
            var knownTokens = Tokeniser.Tokenise(known).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var shared = knownTokens.Intersect(tokens, StringComparer.OrdinalIgnoreCase).Count();

            if (shared >= 3 || known.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                match = known;
                return true;
            }
        }

        match = null;
        return false;
    }
}

/// <summary>
/// Filters vendor marketing and non-topic chatter.
///
/// This matters more than it sounds: in a sample of 30 rundown links, 12 were
/// JetBrains product announcements. Without this filter the scout's output is
/// dominated by IDE release notes, which are never blog topics here.
/// </summary>
public static partial class NoiseFilter
{
    private static readonly string[] NoiseTitlePatterns =
    [
        @"\brelease candidate\b",
        @"\bcall for speakers\b",
        @"\bwebinar\b",
        @"\bpodcast\b",
        @"\broundup\b",
        @"\bdotinsights\b",
        @"\bservicing (releases|updates)\b",
        @"\bnewsletter\b",
        @"\bconference\b",
        @"\bc\+\+\b",
        @"\bgamedev\b",
        @"\bunity\b",
        @"\bwebinar\b",
        @"\bnow available for download\b"
    ];

    /// <summary>Hosts whose output is product marketing rather than technique.</summary>
    private static readonly string[] VendorProductHosts = ["blog.jetbrains.com"];

    private static readonly string[] VendorProductHints =
    [
        "rider", "resharper", "visual studio 20", "dotmemory", "dottrace", "dotpeek", "teamcity"
    ];

    public static bool IsVendorHost(string host) =>
        VendorProductHosts.Contains(host, StringComparer.OrdinalIgnoreCase);

    public static bool IsNoise(TopicCandidate candidate, out string? reason)
    {
        foreach (var pattern in NoiseTitlePatterns)
        {
            if (Regex.IsMatch(candidate.Title, pattern, RegexOptions.IgnoreCase))
            {
                reason = $"matched noise pattern /{pattern}/";
                return true;
            }
        }

        if (VendorProductHosts.Contains(candidate.Host, StringComparer.OrdinalIgnoreCase))
        {
            var title = candidate.Title.ToLowerInvariant();
            if (VendorProductHints.Any(title.Contains))
            {
                reason = "vendor product announcement";
                return true;
            }
        }

        reason = null;
        return false;
    }
}

