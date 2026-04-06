using System.Reflection;
using System.Text.RegularExpressions;
using SharpForge.Client.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SharpForge.Client.Services;

/// <summary>
/// Reads course markdown files from embedded resources (Content/Courses/*.md),
/// parses YAML frontmatter into <see cref="CourseDetail"/> instances.
/// The markdown body contains the raw SVG icon markup.
/// </summary>
public partial class CourseService
{
    private static readonly Assembly Assembly = typeof(CourseService).Assembly;
    private readonly IDeserializer _yamlDeserializer;

    private Dictionary<string, CourseDetail>? _courses;

    public CourseService()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>Returns a single course by slug, or null if not found.</summary>
    public CourseDetail? GetCourse(string slug)
    {
        EnsureLoaded();
        return _courses!.GetValueOrDefault(slug);
    }

    /// <summary>Returns all courses ordered by level then title.</summary>
    public IReadOnlyList<CourseDetail> GetAllCourses()
    {
        EnsureLoaded();
        return _courses!.Values
            .OrderBy(c => c.CourseLevel)
            .ThenBy(c => c.Title)
            .ToList();
    }

    private void EnsureLoaded()
    {
        if (_courses is not null) return;

        var courses = new Dictionary<string, CourseDetail>(StringComparer.OrdinalIgnoreCase);
        var resourceNames = Assembly.GetManifestResourceNames()
            .Where(n => CourseResourceRegex().IsMatch(n));

        foreach (var resourceName in resourceNames)
        {
            using var stream = Assembly.GetManifestResourceStream(resourceName);
            if (stream is null) continue;

            using var reader = new StreamReader(stream);
            var markdown = reader.ReadToEnd();

            var course = Parse(markdown);
            if (!string.IsNullOrEmpty(course.Slug))
            {
                courses[course.Slug] = course;
            }
        }

        _courses = courses;
    }

    private CourseDetail Parse(string markdown)
    {
        var (yaml, body) = SplitFrontmatter(markdown);

        var course = string.IsNullOrWhiteSpace(yaml)
            ? new CourseDetail()
            : _yamlDeserializer.Deserialize<CourseDetail>(yaml);

        // Body is raw SVG — assign directly, no Markdig rendering needed
        course.IconSvg = body.Trim();

        return course;
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

    /// <summary>Matches embedded resources under Content.Courses that end with .md</summary>
    [GeneratedRegex(@"\.Content\.Courses\.[^.]+\.md$", RegexOptions.IgnoreCase)]
    private static partial Regex CourseResourceRegex();

    [GeneratedRegex(@"^---\s*$", RegexOptions.Multiline)]
    private static partial Regex DelimiterRegex();
}

