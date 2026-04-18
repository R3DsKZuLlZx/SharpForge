using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonTakeaways
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
