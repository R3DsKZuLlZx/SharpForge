using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class CourseLearningOutcomes
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
