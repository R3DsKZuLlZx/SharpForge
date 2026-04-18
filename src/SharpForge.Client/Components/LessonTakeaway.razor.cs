using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonTakeaway
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
