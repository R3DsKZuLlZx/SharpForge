using SharpForge.Client.Models;

namespace SharpForge.Client.Pages;

public partial class Blog
{
    private int CurrentPage { get; set; } = 1;
    private const int PageSize = 6;
    private string? SelectedCategory { get; set; }
    private BlogPostFrontmatter? _featured;
    private IReadOnlyList<BlogPostFrontmatter> _allPosts = [];
    private IReadOnlyList<BlogPostFrontmatter> _regularPosts = [];

    protected override void OnInitialized()
    {
        _allPosts = MarkdownService.GetAllPosts();
        _featured = MarkdownService.GetFeaturedPost();
        _regularPosts = MarkdownService.GetRegularPosts();
    }

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)FilteredPosts.Count() / PageSize));

    private IEnumerable<BlogPostFrontmatter> FilteredPosts => SelectedCategory == null
        ? _regularPosts
        : _regularPosts.Where(p => p.Category == SelectedCategory);

    private IEnumerable<BlogPostFrontmatter> CurrentPagePosts => FilteredPosts
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize);

    private Dictionary<string, int> Categories => _regularPosts
        .GroupBy(p => p.Category)
        .ToDictionary(g => g.Key, g => g.Count());

    private void SelectCategory(string? category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
    }

    private void GoToPage(int page)
    {
        CurrentPage = page;
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages) CurrentPage++;
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1) CurrentPage--;
    }
}
