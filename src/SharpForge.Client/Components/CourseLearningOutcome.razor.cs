using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class CourseLearningOutcome
{
    [Parameter]
    public string Text { get; set; } = "";
}
