using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogTag
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string? Class { get; set; }
}
