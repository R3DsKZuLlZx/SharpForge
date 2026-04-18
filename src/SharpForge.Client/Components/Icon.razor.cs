using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class Icon
{
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public int Size { get; set; } = 24;
}

