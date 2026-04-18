namespace SharpForge.Client.Models;

public class BlogPostFrontmatter
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Date { get; set; } = "";
    public string ReadTime { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public List<BlogPostSidebarEntry> Sidebar { get; set; } = [];

    public string Url => $"blog/{Slug}";
    public bool IsFeatured => Category.Equals("Featured", StringComparison.OrdinalIgnoreCase);

    public IEnumerable<BlogSidebarItem> ToSidebarItems() 
        => Sidebar.Select(s => new BlogSidebarItem(s.Href, s.Text));
}
