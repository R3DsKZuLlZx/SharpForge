namespace SharpForge.Client.Models;

public class LessonContent
{
    public LessonFrontmatter Frontmatter { get; init; } = new();
    public string HtmlBody { get; init; } = "";
}

