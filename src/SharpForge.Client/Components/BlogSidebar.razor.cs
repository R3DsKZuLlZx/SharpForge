using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogSidebar
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IEnumerable<BlogSidebarItem>? Items { get; set; }
}
