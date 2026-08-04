using System.Diagnostics;
using System.Text;
using SharpForge.TopicScout.Model;

namespace SharpForge.TopicScout.Publishing;

/// <summary>
/// Files surviving candidates as <c>topic:proposed</c> GitHub issues via the
/// GitHub CLI.
///
/// Everything filed lands in the *proposed* state — never *approved* — so the
/// scout can never cause a post to be drafted without a human triaging it
/// first.
/// </summary>
public sealed class IssueFiler(string workingDirectory, bool dryRun)
{
    public async Task<int> FileAsync(IReadOnlyList<ScoredCandidate> candidates, CancellationToken ct)
    {
        var filed = 0;

        foreach (var candidate in candidates)
        {
            var title = BuildTitle(candidate);
            var body = BuildBody(candidate);

            if (dryRun)
            {
                Console.WriteLine($"  [dry-run] would file: {title}");
                filed++;
                continue;
            }

            var url = await RunGhAsync(title, body, ct);

            if (url is null)
            {
                Console.WriteLine($"  ✗ failed to file: {title}");
                continue;
            }

            Console.WriteLine($"  ✓ {url}  {title}");
            filed++;
        }

        return filed;
    }

    private static string BuildTitle(ScoredCandidate c)
    {
        var prefix = c.Kind == CandidateKind.Decay ? "[topic][decay]" : "[topic]";
        var title = c.Candidate.Title.Trim();

        // GitHub titles are capped at 256 chars; leave room for the prefix.
        if (title.Length > 180)
        {
            title = title[..177] + "...";
        }

        return $"{prefix} {title}";
    }

    /// <summary>
    /// Mirrors the headings emitted by the blog-post issue form, so scout-filed
    /// and hand-filed briefs are shaped identically and the write-blog-post
    /// skill can read either.
    /// </summary>
    private static string BuildBody(ScoredCandidate c)
    {
        var sb = new StringBuilder();

        sb.AppendLine("### Topic").AppendLine();
        sb.AppendLine(c.Candidate.Title).AppendLine();

        sb.AppendLine("### Angle").AppendLine();

        if (c.Kind == CandidateKind.Decay)
        {
            sb.AppendLine($"⚠️ **Possible decay.** This overlaps your existing post " +
                          $"[`{c.MatchedSlug}`](/blog/{c.MatchedSlug}), which is **{c.PostAgeDays} days old**.")
              .AppendLine()
              .AppendLine($"Matched on: {string.Join(", ", c.MatchedTerms.Take(6))}.")
              .AppendLine()
              .AppendLine("**Published posts are never edited.** Decide between exactly two outcomes:")
              .AppendLine()
              .AppendLine($"- Something **fundamental** changed and `{c.MatchedSlug}` is now misleading " +
                          "→ approve this as a **brand-new post** that supersedes it.")
              .AppendLine("- The change is minor → **reject this**, and label it `topic:rejected`.")
              .AppendLine()
              .AppendLine("There is no \"refresh the old post\" option.");
        }
        else
        {
            sb.AppendLine("Nothing in the back catalogue covers this.");
        }

        sb.AppendLine()
          .AppendLine("> 🤖 **The scout cannot supply an angle — it only detects relevance.**")
          .AppendLine("> Add one before approving, or reject this. A post without an angle is")
          .AppendLine("> indistinguishable from every other post on the subject.")
          .AppendLine();

        sb.AppendLine("### Category").AppendLine();
        sb.AppendLine(c.Kind == CandidateKind.Decay && c.MatchedSlug is not null
            ? $"_Needs triage_ — likely the same category as `{c.MatchedSlug}`."
            : "_Needs triage._").AppendLine();

        sb.AppendLine("### Post type").AppendLine();
        sb.AppendLine("News-pegged (tied to a release or spec change)").AppendLine();

        sb.AppendLine("### Sources").AppendLine();
        sb.AppendLine($"- {c.Candidate.Url}").AppendLine();

        sb.AppendLine("### Notes").AppendLine();
        sb.AppendLine($"- Surfaced automatically by `tools/SharpForge.TopicScout`.");
        sb.AppendLine($"- Source feed: {c.Candidate.SourceName}");
        sb.AppendLine($"- Published: {(c.Candidate.AgeDays is { } d ? $"{d} days ago" : "unknown")}");
        sb.AppendLine($"- Relevance score: {c.Score:0.0} ({c.Kind})");
        sb.AppendLine();
        sb.AppendLine("**Verify all technical claims against the linked source before drafting.**");
        sb.AppendLine();
        sb.AppendLine("_Close this issue with the `topic:rejected` label if it is not worth writing —");
        sb.AppendLine("that label is what stops the scout proposing it again._");

        return sb.ToString();
    }

    private async Task<string?> RunGhAsync(string title, string body, CancellationToken ct)
    {
        var bodyFile = Path.Combine(Path.GetTempPath(), $"scout-{Guid.NewGuid():N}.md");

        try
        {
            await File.WriteAllTextAsync(bodyFile, body, ct);

            var psi = new ProcessStartInfo("gh")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            foreach (var a in new[]
                     {
                         "issue", "create",
                         "--title", title,
                         "--body-file", bodyFile,
                         "--label", "blog post",
                         "--label", "topic:proposed",
                         "--label", "source:scout"
                     })
            {
                psi.ArgumentList.Add(a);
            }

            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return null;
            }

            var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
            var stderr = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
            {
                Console.WriteLine($"      gh: {stderr.Trim()}");
                return null;
            }

            return stdout.Trim().Split('\n').LastOrDefault()?.Trim();
        }
        finally
        {
            try { File.Delete(bodyFile); } catch { /* best effort */ }
        }
    }
}

