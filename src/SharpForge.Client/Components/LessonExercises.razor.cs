using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonExercises
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
