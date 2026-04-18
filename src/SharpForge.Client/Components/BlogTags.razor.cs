using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogTags
{
    [Parameter]
    public IEnumerable<string>? Tags { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
