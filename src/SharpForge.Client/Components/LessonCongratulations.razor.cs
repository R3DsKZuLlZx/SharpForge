using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonCongratulations
{
    [Parameter]
    public string CourseTitle { get; set; } = "";

    [Parameter]
    public List<string> Topics { get; set; } = [];

    [Parameter]
    public string ClosingMessage { get; set; } =
        "<strong>What's next?</strong> Continue your journey with more courses!";
}
