using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogTocItem
{
    [Parameter]
    public string Href { get; set; } = "#";

    [Parameter]
    public string Text { get; set; } = string.Empty;
}
