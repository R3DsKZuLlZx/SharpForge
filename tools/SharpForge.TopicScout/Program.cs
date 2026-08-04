using System.Diagnostics;
using System.Text.Json;
using SharpForge.TopicScout.Catalogue;
using SharpForge.TopicScout.Model;
using SharpForge.TopicScout.Publishing;
using SharpForge.TopicScout.Sources;

namespace SharpForge.TopicScout;

public static class Program
{
    private static readonly ITopicSource[] Sources =
    [
        new DailyRundownSource(),
        new MorningBrewSource()
    ];

    public static async Task<int> Main(string[] args)
    {
        var maxAgeDays = GetIntArg(args, "--days") ?? 21;
        var maxIssues = GetIntArg(args, "--max-issues") ?? 5;
        var minScore = GetDoubleArg(args, "--min-score") ?? 2.0;
        var showNoise = args.Contains("--show-noise");
        var createIssues = args.Contains("--create-issues");
        var dryRun = args.Contains("--dry-run");

        var root = FindRepositoryRoot();
        Console.WriteLine($"SharpForge topic scout");
        Console.WriteLine($"repository : {root}");
        Console.WriteLine($"window     : last {maxAgeDays} days");
        Console.WriteLine($"filing     : {(createIssues ? $"on (max {maxIssues}, min score {minScore:0.0})" : "off — report only")}{(dryRun ? " [dry-run]" : "")}");
        Console.WriteLine();

        var catalogue = BackCatalogue.Load(root);
        var knownTopics = await LoadKnownTopicsAsync(root);

        Console.WriteLine($"back catalogue : {catalogue.Count} posts");
        Console.WriteLine($"topic queue    : {knownTopics.Count} known topics (for dedupe)");
        Console.WriteLine();

        using var http = CreateHttpClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        var results = await Task.WhenAll(Sources.Select(s => s.FetchAsync(http, cts.Token)));

        ReportSourceHealth(results);

        var scorer = new RelevanceScorer(catalogue, knownTopics);

        var scored = results
            .SelectMany(r => r.Candidates)
            .Where(c => c.AgeDays is null || c.AgeDays <= maxAgeDays)
            .GroupBy(c => c.Url, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .Select(scorer.Score)
            .ToList();

        Report("DECAY — existing posts these may contradict", scored, CandidateKind.Decay);
        Report("GAP — no post covers these", scored, CandidateKind.Gap);

        var dupes = scored.Count(s => s.Kind == CandidateKind.Duplicate);
        var noise = scored.Count(s => s.Kind == CandidateKind.Noise);
        Console.WriteLine($"filtered: {noise} noise, {dupes} already queued");

        if (showNoise)
        {
            Console.WriteLine();
            foreach (var s in scored.Where(x => x.Kind is CandidateKind.Noise or CandidateKind.Duplicate))
            {
                Console.WriteLine($"  [{s.Kind}] {s.Candidate.Title}  ({s.Reason})");
            }
        }

        if (createIssues)
        {
            await FileIssuesAsync(root, scored, maxIssues, minScore, dryRun, cts.Token);
        }

        return results.Any(r => r.Failed) ? 1 : 0;
    }

    /// <summary>
    /// Files the strongest candidates as <c>topic:proposed</c> issues.
    ///
    /// Two guardrails, because this runs unattended: a score floor so weak
    /// matches are never filed, and a hard cap so a parser regression cannot
    /// flood the repository. DECAY is filed ahead of GAP — a post that has gone
    /// stale is more urgent than one that was never written.
    /// </summary>
    private static async Task FileIssuesAsync(
        string root,
        IReadOnlyList<ScoredCandidate> scored,
        int maxIssues,
        double minScore,
        bool dryRun,
        CancellationToken ct)
    {
        var toFile = scored
            .Where(s => s.Kind is CandidateKind.Decay or CandidateKind.Gap)
            .Where(s => s.Score >= minScore)
            .OrderByDescending(s => s.Kind == CandidateKind.Decay)
            .ThenByDescending(s => s.Score)
            .Take(maxIssues)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"=== filing {toFile.Count} issue(s) as topic:proposed ===");

        if (toFile.Count == 0)
        {
            Console.WriteLine("  (nothing above the score floor)");
            return;
        }

        var filer = new IssueFiler(root, dryRun);
        var filed = await filer.FileAsync(toFile, ct);

        var skipped = scored.Count(s => s.Kind is CandidateKind.Decay or CandidateKind.Gap) - toFile.Count;
        Console.WriteLine();
        Console.WriteLine($"filed {filed}, skipped {skipped} (below score floor or over the cap)");
        Console.WriteLine("Triage with: gh issue list --label \"blog post\" --label \"topic:proposed\"");
    }

    private static void ReportSourceHealth(IReadOnlyList<SourceResult> results)
    {
        Console.WriteLine("=== source health ===");

        foreach (var r in results)
        {
            if (r.Failed)
            {
                Console.WriteLine($"  ✗ {r.SourceName,-26} FAILED: {r.Error}");
                continue;
            }

            // A dead feed must never look like a working one that found nothing.
            var status = r.IsStale ? $"STALE ({r.StaleDays}d)" : "ok";
            var marker = r.IsStale ? "⚠" : "✓";

            Console.WriteLine(
                $"  {marker} {r.SourceName,-26} {r.Candidates.Count,3} links  newest={r.NewestItem?.ToString("yyyy-MM-dd") ?? "unknown"}  {status}");

            if (r.IsStale)
            {
                Console.WriteLine(
                    $"      └─ no items in {r.StaleDays} days — treat this source as dormant, not empty.");
            }
        }

        Console.WriteLine();
    }

    private static void Report(string heading, IReadOnlyList<ScoredCandidate> all, CandidateKind kind)
    {
        var items = all.Where(s => s.Kind == kind)
            .OrderByDescending(s => s.Score)
            .ToList();

        Console.WriteLine($"=== {heading} ({items.Count}) ===");

        if (items.Count == 0)
        {
            Console.WriteLine("  (none)");
            Console.WriteLine();
            return;
        }

        foreach (var s in items)
        {
            var age = s.Candidate.AgeDays is { } d ? $"{d}d" : "?";
            Console.WriteLine($"  [{s.Score,4:0.0}] {s.Candidate.Title}");
            Console.WriteLine($"         {age,-5} {s.Candidate.Host}  — {s.Reason}");

            if (s.MatchedSlug is not null)
            {
                Console.WriteLine($"         ↳ your post '{s.MatchedSlug}' is {s.PostAgeDays}d old");
            }

            Console.WriteLine($"         {s.Candidate.Url}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Reads existing topic issue titles via the GitHub CLI so the scout does not
    /// re-propose something already queued — or already rejected. Best-effort:
    /// if gh is unavailable the scout still runs, just without dedupe.
    /// </summary>
    private static async Task<IReadOnlyList<string>> LoadKnownTopicsAsync(string workingDirectory)
    {
        try
        {
            var psi = new ProcessStartInfo("gh")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            foreach (var a in new[]
                     {
                         "issue", "list", "--label", "blog post", "--state", "all",
                         "--limit", "200", "--json", "title"
                     })
            {
                psi.ArgumentList.Add(a);
            }

            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return [];
            }

            var json = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0 || string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray()
                .Select(e => e.GetProperty("title").GetString() ?? "")
                .Where(t => t.Length > 0)
                .ToList();
        }
        catch
        {
            Console.WriteLine("  (gh unavailable — skipping queue dedupe)");
            return [];
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("SharpForge-TopicScout/0.1 (+https://github.com/R3DsKZuLlZx/SharpForge)");
        return http;
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SharpForge.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
               ?? throw new InvalidOperationException("Could not locate repository root (SharpForge.slnx).");
    }

    private static int? GetIntArg(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out var v) ? v : null;
    }

    private static double? GetDoubleArg(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length && double.TryParse(args[i + 1], out var v) ? v : null;
    }
}

