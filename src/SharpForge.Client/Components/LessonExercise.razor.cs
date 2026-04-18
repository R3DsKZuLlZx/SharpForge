using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonExercise
{
    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
