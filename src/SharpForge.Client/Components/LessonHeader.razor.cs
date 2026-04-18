using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class LessonHeader
{
    [Parameter]
    public int LessonNumber { get; set; }

    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public string Duration { get; set; } = "20 min";

    [Parameter]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
}
