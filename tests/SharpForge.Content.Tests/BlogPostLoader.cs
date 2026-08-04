using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SharpForge.Content.Tests;

/// <summary>
/// Frontmatter shape as authored in <c>Content/Blog/*.md</c>.
/// Mirrors <c>SharpForge.Client.Models.BlogPostFrontmatter</c>, but is
/// deliberately independent: these tests validate the *authoring contract* of
/// the Markdown files themselves, so they must not depend on the Blazor
/// WebAssembly project compiling.
/// </summary>
public sealed class PostFrontmatter
{
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Date { get; set; } = "";
    public string ReadTime { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public List<SidebarEntry> Sidebar { get; set; } = [];
}

public sealed class SidebarEntry
{
    public string Href { get; set; } = "";
    public string Text { get; set; } = "";
}

/// <summary>A single discovered blog post, split into frontmatter and body.</summary>
public sealed record BlogPost(
    string Slug,
    string FilePath,
    string RawYaml,
    string Body,
    PostFrontmatter? Frontmatter,
    string? ParseError)
{
    public override string ToString() => Slug;
}

public static partial class BlogPostLoader
{
    private static readonly Lazy<IReadOnlyList<BlogPost>> Posts = new(Load);

    /// <summary>All posts discovered in <c>src/SharpForge.Client/Content/Blog</c>.</summary>
    public static IReadOnlyList<BlogPost> All => Posts.Value;

    /// <summary>xUnit MemberData feed — one case per post file.</summary>
    public static TheoryData<BlogPost> AllPosts()
    {
        var data = new TheoryData<BlogPost>();
        foreach (var post in All)
        {
            data.Add(post);
        }

        return data;
    }

    public static string ContentDirectory =>
        Path.Combine(RepositoryRoot, "src", "SharpForge.Client", "Content", "Blog");

    /// <summary>
    /// Walks up from the test assembly location to the directory containing
    /// <c>SharpForge.slnx</c>, so tests work from any working directory.
    /// </summary>
    public static string RepositoryRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SharpForge.slnx")))
            {
                dir = dir.Parent;
            }

            return dir?.FullName
                   ?? throw new InvalidOperationException(
                       "Could not locate repository root (no SharpForge.slnx found walking up from "
                       + AppContext.BaseDirectory + ").");
        }
    }

    private static IReadOnlyList<BlogPost> Load()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var posts = new List<BlogPost>();

        foreach (var file in Directory.EnumerateFiles(ContentDirectory, "*.md").OrderBy(f => f))
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var markdown = File.ReadAllText(file);
            var (yaml, body) = SplitFrontmatter(markdown);

            PostFrontmatter? frontmatter = null;
            string? parseError = null;

            if (string.IsNullOrWhiteSpace(yaml))
            {
                parseError = "No YAML frontmatter block found.";
            }
            else
            {
                try
                {
                    frontmatter = deserializer.Deserialize<PostFrontmatter>(yaml);
                }
                catch (Exception ex)
                {
                    parseError = ex.Message;
                }
            }

            posts.Add(new BlogPost(slug, file, yaml, body, frontmatter, parseError));
        }

        return posts;
    }

    /// <summary>
    /// Splits frontmatter from body using the same logic as
    /// <c>MarkdownService.SplitFrontmatter</c>.
    /// </summary>
    private static (string Yaml, string Body) SplitFrontmatter(string markdown)
    {
        const string delimiter = "---";

        if (!markdown.StartsWith(delimiter, StringComparison.Ordinal))
        {
            return ("", markdown);
        }

        var match = DelimiterRegex().Match(markdown).NextMatch();
        if (!match.Success)
        {
            return ("", markdown);
        }

        var yaml = markdown[delimiter.Length..match.Index].Trim();
        var body = markdown[(match.Index + delimiter.Length)..].TrimStart();

        return (yaml, body);
    }

    [GeneratedRegex(@"^---\s*$", RegexOptions.Multiline)]
    private static partial Regex DelimiterRegex();
}

