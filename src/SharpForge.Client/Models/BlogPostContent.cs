namespace SharpForge.Client.Models;

public class BlogPostContent
{
    public BlogPostFrontmatter Frontmatter { get; init; } = new();
    public string HtmlBody { get; init; } = "";
}
