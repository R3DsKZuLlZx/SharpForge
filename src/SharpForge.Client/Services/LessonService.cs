using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;
using SharpForge.Client.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SharpForge.Client.Services;

public partial class LessonService
{
    private static readonly Assembly Assembly = typeof(LessonService).Assembly;
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;

    public LessonService()
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

    /// <summary>
    /// Loads a lesson from an embedded Markdown resource.
    /// Returns null when no embedded resource exists for the given course/lesson.
    /// </summary>
    public LessonContent? GetLesson(string courseSlug, int lessonNumber)
    {
        // .NET converts hyphens in folder names to underscores in resource names
        var normalizedSlug = courseSlug.Replace('-', '_');
        var resourceName = Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".{normalizedSlug}.{lessonNumber}.md", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return null;
        }

        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var markdown = reader.ReadToEnd();

        return Parse(markdown);
    }

    private LessonContent Parse(string markdown)
    {
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

    [GeneratedRegex(@"^---\s*$", RegexOptions.Multiline)]
    private static partial Regex DelimiterRegex();
}

