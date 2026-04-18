using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class BlogPostLayout
{
    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public required string Category { get; set; }

    [Parameter]
    public required string Date { get; set; }

    [Parameter]
    public required string ReadTime { get; set; }

    [Parameter]
    public required string Excerpt { get; set; }

    [Parameter]
    public IEnumerable<string>? Tags { get; set; }

    [Parameter]
    public IEnumerable<BlogSidebarItem>? SidebarItems { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
