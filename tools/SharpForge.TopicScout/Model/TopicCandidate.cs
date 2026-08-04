namespace SharpForge.TopicScout.Model;

/// <summary>A single link surfaced by a topic source, before scoring.</summary>
public sealed record TopicCandidate(
    string Title,
    string Url,
    string SourceName,
    DateTimeOffset? Published)
{
    public string Host => Uri.TryCreate(Url, UriKind.Absolute, out var u)
        ? u.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase)
        : "";

    public int? AgeDays => Published is null
        ? null
        : (int)Math.Max(0, (DateTimeOffset.UtcNow - Published.Value).TotalDays);
}

/// <summary>How a candidate relates to the existing blog.</summary>
public enum CandidateKind
{
    /// <summary>Matches an existing post — that post may now be out of date.</summary>
    Decay,

    /// <summary>No existing post covers this.</summary>
    Gap,

    /// <summary>Filtered out as vendor/product noise.</summary>
    Noise,

    /// <summary>Already present in the topic queue.</summary>
    Duplicate
}

/// <summary>A candidate after relevance scoring against the back catalogue.</summary>
public sealed record ScoredCandidate(
    TopicCandidate Candidate,
    CandidateKind Kind,
    double Score,
    string? MatchedSlug,
    DateTimeOffset? MatchedPostDate,
    IReadOnlyList<string> MatchedTerms,
    string? Reason)
{
    /// <summary>Days between the matched post and the candidate — the staleness signal.</summary>
    public int? PostAgeDays => MatchedPostDate is null
        ? null
        : (int)Math.Max(0, (DateTimeOffset.UtcNow - MatchedPostDate.Value).TotalDays);
}

/// <summary>Outcome of polling one source, including health.</summary>
public sealed record SourceResult(
    string SourceName,
    IReadOnlyList<TopicCandidate> Candidates,
    DateTimeOffset? NewestItem,
    string? Error)
{
    public bool Failed => Error is not null;

    /// <summary>
    /// A feed that has not published in this long is almost certainly dead.
    /// Reported explicitly — a silent dead feed is indistinguishable from a
    /// working one that found nothing.
    /// </summary>
    public const int StaleAfterDays = 60;

    public int? StaleDays => NewestItem is null
        ? null
        : (int)Math.Max(0, (DateTimeOffset.UtcNow - NewestItem.Value).TotalDays);

    public bool IsStale => StaleDays > StaleAfterDays;
}

