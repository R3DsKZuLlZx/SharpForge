using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class CoursePrerequisites
{
    [Parameter]
    public List<string> Prerequisites { get; set; } = [];
}
