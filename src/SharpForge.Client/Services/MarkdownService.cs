using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;
using SharpForge.Client.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SharpForge.Client.Services;

public partial class MarkdownService
{
    private static readonly Assembly Assembly = typeof(MarkdownService).Assembly;
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;

    private Dictionary<string, CourseDetail>? _courses;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers()
            .UseAutoLinks()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseTaskLists()
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public CourseDetail? GetCourse(string slug)
    {
        EnsureCoursesLoaded();
        return _courses!.GetValueOrDefault(slug);
    }

    public IReadOnlyList<CourseDetail> GetAllCourses()
    {
        EnsureCoursesLoaded();
        return _courses!.Values
            .OrderBy(c => c.CourseLevel)
            .ThenBy(c => c.Title)
            .ToList();
    }

    public LessonContent? GetLesson(string courseSlug, int lessonNumber)
    {
        // .NET converts hyphens in folder names to underscores in resource names
        var normalizedSlug = courseSlug.Replace('-', '_');
        var resourceName = Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".{normalizedSlug}.{lessonNumber}.md", StringComparison.OrdinalIgnoreCase)
                                 && n.Contains(".Content.Courses."));

        if (resourceName is null)
        {
            return null;
        }

        var markdown = ReadResource(resourceName);
        if (markdown is null)
        {
            return null;
        }

        var (yaml, body) = SplitFrontmatter(markdown);

        var frontmatter = string.IsNullOrWhiteSpace(yaml)
            ? new LessonFrontmatter()
            : _yamlDeserializer.Deserialize<LessonFrontmatter>(yaml);

        var html = Markdown.ToHtml(body, _pipeline);

        return new LessonContent
        {
            Frontmatter = frontmatter,
            HtmlBody = html
        };
    }
    
    public BlogPostContent? GetPost(string slug)
    {
        var resourceName = Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".{slug}.md", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return null;
        }

        var markdown = ReadResource(resourceName);
        if (markdown is null)
        {
            return null;
        }

        var (yaml, body) = SplitFrontmatter(markdown);

        var frontmatter = string.IsNullOrWhiteSpace(yaml)
            ? new BlogPostFrontmatter()
            : _yamlDeserializer.Deserialize<BlogPostFrontmatter>(yaml);

        var html = Markdown.ToHtml(body, _pipeline);

        return new BlogPostContent
        {
            Frontmatter = frontmatter,
            HtmlBody = html
        };
    }

    private void EnsureCoursesLoaded()
    {
        if (_courses is not null)
        {
            return;
        }

        var courses = new Dictionary<string, CourseDetail>(StringComparer.OrdinalIgnoreCase);
        var resourceNames = Assembly.GetManifestResourceNames()
            .Where(n => CourseResourceRegex().IsMatch(n));

        foreach (var resourceName in resourceNames)
        {
            var markdown = ReadResource(resourceName);
            if (markdown is null)
            {
                continue;
            }

            var (yaml, body) = SplitFrontmatter(markdown);

            var course = string.IsNullOrWhiteSpace(yaml)
                ? new CourseDetail()
                : _yamlDeserializer.Deserialize<CourseDetail>(yaml);

            // Body is raw SVG — assign directly, no Markdig rendering needed
            course.IconSvg = body.Trim();

            if (!string.IsNullOrEmpty(course.Slug))
            {
                courses[course.Slug] = course;
            }
        }

        _courses = courses;
    }

    private static string? ReadResource(string resourceName)
    {
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static (string Yaml, string Body) SplitFrontmatter(string markdown)
    {
        const string delimiter = "---";

        if (!markdown.StartsWith(delimiter))
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

    [GeneratedRegex(@"\.Content\.Courses\.[^.]+\.course\.md$", RegexOptions.IgnoreCase)]
    private static partial Regex CourseResourceRegex();

    [GeneratedRegex(@"^---\s*$", RegexOptions.Multiline)]
    private static partial Regex DelimiterRegex();
}
