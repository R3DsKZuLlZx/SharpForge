using System.Text.RegularExpressions;

namespace SharpForge.Content.Tests;

/// <summary>
/// Guards the consistency of the agent-facing documentation itself.
///
/// AGENTS.md is the single source of truth that both humans and AI agents read.
/// If it drifts from the code, agents follow a spec that is quietly wrong —
/// which is precisely how the broken sidebar anchors got introduced.
/// </summary>
public partial class DocumentationTests
{
    private static string AgentsMd =>
        File.ReadAllText(Path.Combine(BlogPostLoader.RepositoryRoot, "AGENTS.md"));

    [Fact]
    public void AgentsMd_CategoryList_MatchesAllowedCategories()
    {
        // AGENTS.md tells authors to keep these in sync by hand. Assert it,
        // so a forgotten update fails CI instead of silently rejecting a
        // legitimate category (or allowing an undocumented one).
        var documented = CategoryBulletRegex()
            .Matches(AgentsMd)
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var allowed = FrontmatterTests.AllowedCategories.ToHashSet(StringComparer.Ordinal);

        var missingFromDocs = allowed.Except(documented).OrderBy(c => c).ToList();
        var missingFromCode = documented.Except(allowed).OrderBy(c => c).ToList();

        Assert.True(missingFromDocs.Count == 0 && missingFromCode.Count == 0,
            "AGENTS.md 'Existing categories' and FrontmatterTests.AllowedCategories are out of sync. "
            + (missingFromDocs.Count > 0
                ? $"Missing from AGENTS.md: {string.Join(", ", missingFromDocs)}. "
                : "")
            + (missingFromCode.Count > 0
                ? $"Missing from AllowedCategories: {string.Join(", ", missingFromCode)}."
                : ""));
    }

    [Theory]
    [InlineData(".agents/skills/write-blog-post/SKILL.md")]
    [InlineData(".claude/skills/write-blog-post/SKILL.md")]
    [InlineData(".github/prompts/write-blog-post.prompt.md")]
    public void SkillFile_Exists(string relativePath)
    {
        var full = Path.Combine(BlogPostLoader.RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(full),
            $"Expected agent skill file at {relativePath}. Tool-specific wrappers must accompany "
            + "the canonical skill in .agents/skills/ — see the 'Agent skills' section of AGENTS.md.");
    }

    [Theory]
    [InlineData(".claude/skills/write-blog-post/SKILL.md")]
    [InlineData(".github/prompts/write-blog-post.prompt.md")]
    public void SkillWrapper_PointsAtCanonicalSkill(string relativePath)
    {
        var full = Path.Combine(BlogPostLoader.RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var content = File.ReadAllText(full);

        Assert.Contains(".agents/skills/write-blog-post/SKILL.md", content, StringComparison.Ordinal);

        // Wrappers must stay thin — if one grows, the instructions have been
        // duplicated and the two agents will start behaving differently.
        var lines = content.Split('\n').Length;
        Assert.True(lines <= 40,
            $"{relativePath} is {lines} lines. Wrappers should just point at the canonical skill; "
            + "put real guidance in .agents/skills/write-blog-post/SKILL.md instead.");
    }

    [Theory]
    [InlineData("AGENTS.md")]
    [InlineData(".agents/skills/write-blog-post/SKILL.md")]
    public void ImmutabilityPolicy_IsStated(string relativePath)
    {
        // Published posts are a dated record. Guidance that suggests "updating"
        // or "refreshing" an existing post has leaked in before — this pins it.
        var full = Path.Combine(BlogPostLoader.RepositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var content = File.ReadAllText(full);

        Assert.True(
            content.Contains("immutable", StringComparison.OrdinalIgnoreCase),
            $"{relativePath} must state that published posts are immutable.");

        Assert.True(
            content.Contains("supersede", StringComparison.OrdinalIgnoreCase),
            $"{relativePath} must describe the supersede-or-skip rule.");
    }

    [Theory]
    [InlineData(".agents/skills/write-blog-post/SKILL.md")]
    [InlineData("tools/SharpForge.TopicScout/Publishing/IssueFiler.cs")]
    [InlineData(".github/workflows/draft-blog-post.yml")]
    public void NoGuidanceSuggestsEditingAnExistingPost(string relativePath)
    {
        var full = Path.Combine(BlogPostLoader.RepositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var content = File.ReadAllText(full);

        // Phrasings that previously told agents to revise a published post.
        string[] banned =
        [
            "update to that post",
            "an *update* rather than a new post",
            "instead of a new one",
            "refresh the existing",
            "rewrite the existing post"
        ];

        var found = banned.Where(b => content.Contains(b, StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.True(found.Count == 0,
            $"{relativePath} suggests editing a published post: {string.Join("; ", found)}. "
            + "Posts are immutable — a significant change warrants a brand-new superseding post, "
            + "and anything less warrants nothing.");
    }

    [Fact]
    public void IssueForm_CategoryOptions_MatchAllowedCategories()
    {
        // The blog-post issue form is where topics enter the queue. If its
        // dropdown drifts from the allowed list, a topic gets approved with a
        // category the validator will later reject.
        var formPath = Path.Combine(BlogPostLoader.RepositoryRoot,
            ".github", "ISSUE_TEMPLATE", "blog-post.yml");

        Assert.True(File.Exists(formPath), $"Blog post issue form not found at {formPath}");

        var options = CategoryOptionRegex()
            .Match(File.ReadAllText(formPath))
            .Groups[1].Value
            .Split('\n')
            .Select(l => l.Trim().TrimStart('-').Trim().Trim('"'))
            .Where(l => l.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(options);

        // 'Featured' is deliberately excluded — it is a pinned-post marker,
        // not something anyone should pick when proposing a topic.
        var expected = FrontmatterTests.AllowedCategories
            .Where(c => c != "Featured")
            .ToHashSet(StringComparer.Ordinal);

        var missingFromForm = expected.Except(options).OrderBy(c => c).ToList();
        var unexpectedInForm = options.Except(expected).OrderBy(c => c).ToList();

        Assert.True(missingFromForm.Count == 0 && unexpectedInForm.Count == 0,
            "blog-post.yml category dropdown is out of sync with FrontmatterTests.AllowedCategories. "
            + (missingFromForm.Count > 0 ? $"Missing from form: {string.Join(", ", missingFromForm)}. " : "")
            + (unexpectedInForm.Count > 0 ? $"Unexpected in form: {string.Join(", ", unexpectedInForm)}." : ""));
    }

    [GeneratedRegex(@"^- `([A-Za-z#. ]+)`(?: \(reserved[^\r\n]*\))?\r?$", RegexOptions.Multiline)]
    private static partial Regex CategoryBulletRegex();

    /// <summary>Captures the options block of the <c>category</c> dropdown in the issue form.</summary>
    [GeneratedRegex(@"id:\s*category\b.*?options:\r?\n((?:\s*-\s*[^\r\n]+\r?\n)+)",
        RegexOptions.Singleline)]
    private static partial Regex CategoryOptionRegex();
}

