using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;
using SharpForge.Client.Services;

namespace SharpForge.Client.Pages;

public partial class BlogPost
{
    [Inject]
    public required BlogService BlogService { get; set; }

    [Parameter]
    public string Slug { get; set; } = "";

    private BlogPostContent? _post;

    protected override void OnParametersSet()
    {
        _post = BlogService.GetPost(Slug);
    }
}
