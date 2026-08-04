using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace SharpForge.Content.Tests;

/// <summary>
/// Validates that every sidebar <c>href</c> resolves to a real heading anchor.
///
/// Rather than reimplementing Markdig's auto-identifier slug algorithm (which
/// AGENTS.md only documents by example), these tests build the *same* pipeline
/// MarkdownService uses and read the generated ids straight off the AST. That
/// makes the assertion authoritative — if Markdig changes its slug rules, the
/// tests track it automatically.
/// </summary>
public class SidebarAnchorTests
{
    /// <summary>Identical to the pipeline configured in MarkdownService.</summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAutoIdentifiers()
        .UseAutoLinks()
        .UsePipeTables()
        .UseEmphasisExtras()
        .UseTaskLists()
        .Build();

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_SidebarHrefsStartWithHash(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        var malformed = fm!.Sidebar
            .Where(s => !s.Href.StartsWith('#'))
            .Select(s => s.Href)
            .ToList();

        Assert.True(malformed.Count == 0,
            $"{post.Slug}.md: sidebar href(s) must start with '#': {string.Join(", ", malformed)}");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_SidebarHrefsResolveToRealHeadings(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        var headingIds = GetHeadingIds(post.Body);

        var broken = fm!.Sidebar
            .Select(s => s.Href.TrimStart('#'))
            .Where(anchor => !headingIds.Contains(anchor))
            .ToList();

        Assert.True(broken.Count == 0,
            $"{post.Slug}.md: sidebar anchor(s) do not match any heading id: "
            + $"{string.Join(", ", broken.Select(b => "#" + b))}. "
            + $"Available heading ids: {string.Join(", ", headingIds.Select(h => "#" + h))}");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_SidebarEntriesAreUnique(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        var duplicates = fm!.Sidebar
            .GroupBy(s => s.Href, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.True(duplicates.Count == 0,
            $"{post.Slug}.md: duplicate sidebar href(s): {string.Join(", ", duplicates)}");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_SidebarEntriesHaveText(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        var missing = fm!.Sidebar
            .Where(s => string.IsNullOrWhiteSpace(s.Text))
            .Select(s => s.Href)
            .ToList();

        Assert.True(missing.Count == 0,
            $"{post.Slug}.md: sidebar entries missing 'text': {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_SidebarCoversTopLevelSections(BlogPost post)
    {
        var fm = post.Frontmatter;
        Assert.True(fm is not null, $"{post.Slug}.md: unparseable frontmatter — {post.ParseError}");

        // AGENTS.md: "Typically 5-7 entries mapping to the main '## ' headings."
        // We assert the sidebar isn't wildly out of step with the H2 structure
        // rather than demanding an exact match, so a post can omit a minor section.
        var h2Count = Markdown.Parse(post.Body, Pipeline)
            .Descendants<HeadingBlock>()
            .Count(h => h.Level == 2);

        Assert.True(h2Count > 0, $"{post.Slug}.md: no '## ' headings found.");

        Assert.True(fm!.Sidebar.Count >= Math.Min(3, h2Count),
            $"{post.Slug}.md: sidebar has {fm.Sidebar.Count} entries for {h2Count} '## ' sections — "
            + "the on-page table of contents will look sparse.");

        Assert.True(fm.Sidebar.Count <= h2Count,
            $"{post.Slug}.md: sidebar has {fm.Sidebar.Count} entries but only {h2Count} "
            + "'## ' sections exist.");
    }

    private static HashSet<string> GetHeadingIds(string body)
    {
        return Markdown.Parse(body, Pipeline)
            .Descendants<HeadingBlock>()
            .Select(h => h.GetAttributes().Id)
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .ToHashSet(StringComparer.Ordinal);
    }
}

