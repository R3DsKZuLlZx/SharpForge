using System.Globalization;
using System.Text.RegularExpressions;

namespace SharpForge.Content.Tests;

/// <summary>
/// Validates the YAML frontmatter contract documented in AGENTS.md.
/// These run against every file in Content/Blog, so an AI-drafted post that
/// breaks the schema fails CI rather than silently breaking the site.
/// </summary>
public partial class FrontmatterTests
{
    /// <summary>
    /// Allowed <c>category</c> values. Must stay in sync with the
    /// "Existing categories" list in AGENTS.md.
    /// </summary>
    public static readonly string[] AllowedCategories =
    [
        "AI",
        "Architecture",
        "ASP.NET Core",
        "Best Practices",
        "Blazor",
        "C#",
        "DevOps",
        "Entity Framework",
        "Featured",
        "Performance",
        "Testing"
    ];

    [Fact]
    public void ContentDirectory_ContainsPosts()
    {
        Assert.True(Directory.Exists(BlogPostLoader.ContentDirectory),
            $"Blog content directory not found at {BlogPostLoader.ContentDirectory}");

        Assert.NotEmpty(BlogPostLoader.All);
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasParseableFrontmatter(BlogPost post)
    {
        Assert.True(post.ParseError is null,
            $"{post.Slug}.md: frontmatter could not be parsed — {post.ParseError}");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasAllRequiredFields(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        Assert.False(string.IsNullOrWhiteSpace(fm.Title), $"{post.Slug}.md: 'title' is required.");
        Assert.False(string.IsNullOrWhiteSpace(fm.Category), $"{post.Slug}.md: 'category' is required.");
        Assert.False(string.IsNullOrWhiteSpace(fm.Date), $"{post.Slug}.md: 'date' is required.");
        Assert.False(string.IsNullOrWhiteSpace(fm.ReadTime), $"{post.Slug}.md: 'readTime' is required.");
        Assert.False(string.IsNullOrWhiteSpace(fm.Excerpt), $"{post.Slug}.md: 'excerpt' is required.");
        Assert.NotEmpty(fm.Tags);
        Assert.NotEmpty(fm.Sidebar);
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasParseableDate(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        // MarkdownService.ParseDate uses this exact format and silently falls
        // back to DateTime.MinValue — which would sort the post to the bottom.
        var parsed = DateTime.TryParseExact(fm.Date, "MMMM d, yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

        Assert.True(parsed,
            $"{post.Slug}.md: date '{fm.Date}' is not in 'MMMM d, yyyy' format (e.g. 'March 15, 2026'). "
            + "MarkdownService would sort this post to the bottom of the listing.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasValidReadTimeFormat(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        Assert.True(ReadTimeRegex().IsMatch(fm.ReadTime),
            $"{post.Slug}.md: readTime '{fm.ReadTime}' must match 'X min read' (e.g. '10 min read').");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_UsesAllowedCategory(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        Assert.True(AllowedCategories.Contains(fm.Category),
            $"{post.Slug}.md: category '{fm.Category}' is not allowed. "
            + $"Use one of: {string.Join(", ", AllowedCategories)}. "
            + "To add a new category, update both AGENTS.md and FrontmatterTests.AllowedCategories.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasKebabCaseSlug(BlogPost post)
    {
        Assert.True(SlugRegex().IsMatch(post.Slug),
            $"'{post.Slug}.md': file name must be lowercase kebab-case (e.g. 'my-new-post-topic.md'), "
            + "because the file name becomes the URL.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasThreeToFiveTags(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        Assert.True(fm.Tags.Count is >= 3 and <= 5,
            $"{post.Slug}.md: expected 3-5 tags per AGENTS.md, found {fm.Tags.Count} "
            + $"({string.Join(", ", fm.Tags)}).");
    }

    [Fact]
    public void Posts_HaveUniqueSlugs()
    {
        // Slugs are derived from file names, so duplicates can only differ by
        // case — which still collide, since GetPost() matches OrdinalIgnoreCase.
        var duplicates = BlogPostLoader.All
            .GroupBy(p => p.Slug, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.True(duplicates.Count == 0,
            $"Duplicate slugs found (case-insensitive): {string.Join(", ", duplicates)}");
    }

    [Fact]
    public void Posts_HaveExactlyOneFeaturedPost()
    {
        // Blog.razor.cs resolves a single featured post via
        // MarkdownService.GetFeaturedPost() -> FirstOrDefault(p => p.IsFeatured).
        // More than one silently hides posts from the listing page.
        var featured = BlogPostLoader.All
            .Where(p => p.Frontmatter is not null)
            .Where(p => p.Frontmatter!.Category.Equals("Featured", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Slug)
            .ToList();

        Assert.True(featured.Count == 1,
            featured.Count == 0
                ? "No post has category 'Featured'. Exactly one post must be featured."
                : $"{featured.Count} posts use category 'Featured' ({string.Join(", ", featured)}). "
                  + "Only one is allowed — GetFeaturedPost() resolves a single post and the "
                  + "others would be excluded from the regular listing.");
    }

    [Theory]
    [MemberData(nameof(BlogPostLoader.AllPosts), MemberType = typeof(BlogPostLoader))]
    public void Post_HasReasonableExcerptLength(BlogPost post)
    {
        var fm = RequireFrontmatter(post);

        // Shown on the listing card and in the post header — long excerpts
        // break the card layout.
        Assert.True(fm.Excerpt.Length is >= 40 and <= 300,
            $"{post.Slug}.md: excerpt is {fm.Excerpt.Length} characters; "
            + "aim for 1-2 sentences (40-300 characters).");
    }

    private static PostFrontmatter RequireFrontmatter(BlogPost post)
    {
        Assert.True(post.Frontmatter is not null,
            $"{post.Slug}.md: frontmatter could not be parsed — {post.ParseError}");

        return post.Frontmatter!;
    }

    [GeneratedRegex(@"^\d+ min read$")]
    private static partial Regex ReadTimeRegex();

    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}

