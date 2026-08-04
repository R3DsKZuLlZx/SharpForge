using SharpForge.TopicScout.Catalogue;
using SharpForge.TopicScout.Model;

namespace SharpForge.Content.Tests;

/// <summary>
/// Covers the topic scout's heuristics — the fragile, judgement-shaped part of
/// the pipeline. The feed parsers are exercised against the live network by the
/// tool itself; these tests pin the classification rules so tuning one
/// heuristic cannot silently break another.
/// </summary>
public class ScoutHeuristicsTests
{
    private static readonly CataloguePost UnitTestingPost = new(
        "unit-testing-xunit-moq",
        "Unit Testing with xUnit and Moq",
        "Testing",
        DateTimeOffset.Parse("2025-12-28T00:00:00Z"),
        ["xUnit", "Moq", "Unit Testing", "C#"],
        ["Arrange Act Assert", "Mocking Dependencies"]);

    private static readonly CataloguePost SecurityPost = new(
        "securing-aspnet-core-apis",
        "Securing ASP.NET Core APIs",
        "ASP.NET Core",
        DateTimeOffset.Parse("2025-11-25T00:00:00Z"),
        ["Security", "JWT", "Authentication", "ASP.NET Core"],
        ["API Keys", "Rate Limiting"]);

    private static RelevanceScorer Scorer(params string[] knownTopics) =>
        new([UnitTestingPost, SecurityPost], knownTopics);

    private static TopicCandidate Candidate(string title, string url = "https://devblogs.microsoft.com/x", int ageDays = 2) =>
        new(title, url, "test", DateTimeOffset.UtcNow.AddDays(-ageDays));

    [Fact]
    public void Candidate_OverlappingExistingPost_IsDecay()
    {
        var result = Scorer().Score(Candidate("From generated code to trusted code with a unit-test agent"));

        Assert.Equal(CandidateKind.Decay, result.Kind);
        Assert.Equal("unit-testing-xunit-moq", result.MatchedSlug);
    }

    [Fact]
    public void Tokeniser_SplitsHyphensAndStems()
    {
        // "unit-test" must reach the same terms as a post tagged "Unit Testing",
        // otherwise decay detection misses the most obvious matches.
        var tokens = Tokeniser.Tokenise("unit-test agent").ToList();

        Assert.Contains("unit", tokens);
        Assert.Contains("test", tokens);
    }

    [Fact]
    public void Tokeniser_DropsUbiquitousTerms()
    {
        // ".NET" appears in nearly every headline in these feeds; if it counted
        // as a match, everything would match everything.
        var tokens = Tokeniser.Tokenise("Announcing the new .NET release").ToList();

        Assert.DoesNotContain("net", tokens);
        Assert.DoesNotContain("dotnet", tokens);
        Assert.DoesNotContain("announcing", tokens);
    }

    [Fact]
    public void Candidate_UnrelatedToCatalogue_IsGap()
    {
        var result = Scorer().Score(Candidate("P/Invoke Showdown: DllImport vs LibraryImport"));

        Assert.Equal(CandidateKind.Gap, result.Kind);
        Assert.Null(result.MatchedSlug);
    }

    [Fact]
    public void Candidate_AlreadyInQueue_IsDuplicate()
    {
        var result = Scorer("[topic] Migrating to MCP C# SDK v2.0")
            .Score(Candidate("Announcing v2.0 of the official MCP C# SDK"));

        Assert.Equal(CandidateKind.Duplicate, result.Kind);
    }

    [Theory]
    [InlineData("Rider 2026.2 Release Candidate Is Out!")]
    [InlineData("JetBrains GameDev Days 2026 - Call for Speakers")]
    [InlineData("dotInsights - August 2026")]
    [InlineData(".NET and .NET Framework July 2026 servicing releases updates")]
    [InlineData("ReSharper C++ 2026.2: C++26 Reflection")]
    public void VendorAndAdminChatter_IsNoise(string title)
    {
        var result = Scorer().Score(Candidate(title));

        Assert.Equal(CandidateKind.Noise, result.Kind);
    }

    [Fact]
    public void RecentNews_ScoresHigherThanOldNews()
    {
        var fresh = Scorer().Score(Candidate("API security key rotation guidance", ageDays: 1));
        var stale = Scorer().Score(Candidate("API security key rotation guidance", ageDays: 90));

        Assert.True(fresh.Score > stale.Score,
            $"expected recency to raise the score (fresh={fresh.Score}, stale={stale.Score})");
    }

    [Fact]
    public void VendorBlog_IsDemotedRelativeToIndependentBlog()
    {
        var vendor = Scorer().Score(Candidate("Debugging productivity deep dive", "https://blog.jetbrains.com/dotnet/x"));
        var independent = Scorer().Score(Candidate("Debugging productivity deep dive", "https://andrewlock.net/x"));

        Assert.True(vendor.Score < independent.Score,
            $"expected vendor content to be demoted (vendor={vendor.Score}, independent={independent.Score})");
    }

    [Fact]
    public void StaleFeed_IsReportedAsStaleNotEmpty()
    {
        // The Morning Brew stopped publishing in Aug 2024. A dormant source must
        // be visibly dormant — otherwise it looks like a healthy source that
        // simply found nothing this week.
        var dormant = new SourceResult("dormant", [], DateTimeOffset.UtcNow.AddDays(-732), null);
        var healthy = new SourceResult("healthy", [], DateTimeOffset.UtcNow.AddDays(-1), null);

        Assert.True(dormant.IsStale);
        Assert.False(healthy.IsStale);
        Assert.Equal(732, dormant.StaleDays);
    }
}

