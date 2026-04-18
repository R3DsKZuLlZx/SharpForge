using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogHeader
{
    [Parameter]
    public string BackUrl { get; set; } = "blog";

    [Parameter]
    public string Category { get; set; } = string.Empty;

    [Parameter]
    public string Date { get; set; } = string.Empty;

    [Parameter]
    public string ReadTime { get; set; } = string.Empty;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Excerpt { get; set; } = string.Empty;
}
