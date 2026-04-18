using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class BlogAuthor
{
    [Parameter]
    public string AvatarText { get; set; } = "SF";

    [Parameter]
    public string AuthorName { get; set; } = "SharpForge Team";

    [Parameter]
    public string Bio { get; set; } = "Passionate about C# and .NET development";
}
