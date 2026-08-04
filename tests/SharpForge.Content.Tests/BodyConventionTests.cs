using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;

namespace SharpForge.Content.Tests;

/// <summary>
/// Validates the Markdown body conventions in AGENTS.md — the things an LLM
/// gets subtly wrong: unlabelled code fences, HTML entities inside fences,
/// missing closing sections, and readTime that doesn't match the actual length.
/// </summary>
public partial class BodyConventionTests
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAutoIdentifiers()
        .UseAutoLinks()
        .UsePipeTables()
        .UseEmphasisExtras()
        .UseTaskLists()
        .Build();

    private static readonly string[] ValidClosingHeadings =
        ["conclusion", "summary", "best practices", "key takeaways"];

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasBalancedCodeFences(BlogPost post)
    {
        // An unclosed fence swallows the rest of the post silently.
        var fenceCount = post.Body
            .Split('\n')
            .Count(line => line.TrimStart().StartsWith("```", StringComparison.Ordinal));

        Assert.True(fenceCount % 2 == 0,
            $"{post.Slug}.md: found {fenceCount} code fence markers (odd number) — "
            + "a fenced code block is unclosed.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_CodeFencesSpecifyLanguage(BlogPost post)
    {
        var unlabelled = Markdown.Parse(post.Body, Pipeline)
            .Descendants<FencedCodeBlock>()
            .Where(b => string.IsNullOrWhiteSpace(b.Info))
            .Select(b => b.Line + 1)
            .ToList();

        Assert.True(unlabelled.Count == 0,
            $"{post.Slug}.md: code block(s) missing a language identifier at line(s) "
            + $"{string.Join(", ", unlabelled)}. Use ```csharp, ```bash, ```yaml etc.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_CodeBlocksDoNotUseHtmlEntities(BlogPost post)
    {
        // AGENTS.md: use < and > directly in fenced code blocks.
        // LLMs frequently emit &lt;T&gt; for generics, which renders literally.
        var offending = Markdown.Parse(post.Body, Pipeline)
            .Descendants<FencedCodeBlock>()
            .Where(b => HtmlEntityRegex().IsMatch(b.Lines.ToString()))
            .Select(b => b.Line + 1)
            .ToList();

        Assert.True(offending.Count == 0,
            $"{post.Slug}.md: code block(s) at line(s) {string.Join(", ", offending)} contain HTML "
            + "entities (&lt; &gt; &amp; &quot;). Use the literal characters — Markdig handles escaping.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_StartsWithParagraphNotHeading(BlogPost post)
    {
        // AGENTS.md: opening paragraph comes immediately after frontmatter,
        // with no heading before it.
        var firstBlock = Markdown.Parse(post.Body, Pipeline).FirstOrDefault();

        Assert.True(firstBlock is ParagraphBlock,
            $"{post.Slug}.md: body must open with a paragraph, not a "
            + $"{firstBlock?.GetType().Name ?? "empty document"}.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_DoesNotUseH1(BlogPost post)
    {
        // The title comes from frontmatter and is rendered by BlogHeader.
        // An H1 in the body duplicates it.
        var h1s = Markdown.Parse(post.Body, Pipeline)
            .Descendants<HeadingBlock>()
            .Where(h => h.Level == 1)
            .Select(h => h.Line + 1)
            .ToList();

        Assert.True(h1s.Count == 0,
            $"{post.Slug}.md: body contains '# ' heading(s) at line(s) {string.Join(", ", h1s)}. "
            + "The title is rendered from frontmatter — start sections at '## '.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_EndsWithClosingSection(BlogPost post)
    {
        var h2s = Markdown.Parse(post.Body, Pipeline)
            .Descendants<HeadingBlock>()
            .Where(h => h.Level == 2)
            .ToList();

        Assert.True(h2s.Count > 0, $"{post.Slug}.md: no '## ' headings found.");

        // Check the last two sections rather than only the final one: a post may
        // legitimately close with Best Practices followed by a short reference
        // section such as Troubleshooting or FAQ.
        var trailing = h2s
            .TakeLast(2)
            .Select(h => h.Inline?.FirstChild?.ToString()?.Trim().ToLowerInvariant() ?? "")
            .ToList();

        Assert.True(trailing.Any(t => ValidClosingHeadings.Any(t.Contains)),
            $"{post.Slug}.md: final sections are [{string.Join(", ", trailing)}]. AGENTS.md requires "
            + $"the post to wrap up with one of: {string.Join(", ", ValidClosingHeadings)}.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_ReadTimeMatchesLength(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        var stated = int.Parse(ReadTimeValueRegex().Match(fm!.ReadTime).Groups[1].Value);

        var (proseWords, codeLines) = Measure(post.Body);

        // These posts are code-heavy, so prose word count alone badly
        // underestimates them. Model reading as prose at ~200 wpm plus code at
        // ~20 lines/min — code is read far more slowly than prose.
        var estimated = Math.Max(1, (int)Math.Round(proseWords / 200.0 + codeLines / 20.0));

        // Wide tolerance: this is a guardrail against an LLM inventing
        // "15 min read" for a stub, not a precision instrument.
        var lower = Math.Max(1, (int)Math.Floor(estimated * 0.5));
        var upper = (int)Math.Ceiling(estimated * 2.0) + 2;

        Assert.True(stated >= lower && stated <= upper,
            $"{post.Slug}.md: readTime says '{fm.ReadTime}' but the post has ~{proseWords} prose words "
            + $"and {codeLines} lines of code (~{estimated} min). Expected between {lower} and {upper} minutes.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_IsWithinTargetLength(BlogPost post)
    {
        // AGENTS.md targets 100-300 lines. Warn generously — this is a
        // guardrail against a truncated or runaway generation, not a style rule.
        var lines = post.Body.Split('\n').Length;

        Assert.True(lines is >= 50 and <= 500,
            $"{post.Slug}.md: body is {lines} lines; AGENTS.md targets 100-300. "
            + "Check the post isn't truncated or padded.");
    }

    /// <summary>Counts prose words outside fenced code blocks, and code lines inside them.</summary>
    private static (int ProseWords, int CodeLines) Measure(string body)
    {
        var inFence = false;
        var proseWords = 0;
        var codeLines = 0;

        foreach (var line in body.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence)
            {
                codeLines++;
                continue;
            }

            proseWords += line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        return (proseWords, codeLines);
    }

    [GeneratedRegex(@"&(lt|gt|amp|quot|#\d+);")]
    private static partial Regex HtmlEntityRegex();

    [GeneratedRegex(@"^(\d+) min read$")]
    private static partial Regex ReadTimeValueRegex();
}

