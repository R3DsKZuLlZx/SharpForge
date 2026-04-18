using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class CourseHero
{
    [Parameter]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public string Description { get; set; } = "";

    [Parameter]
    public int LessonCount { get; set; }

    [Parameter]
    public string Duration { get; set; } = "";

    [Parameter]
    public string StudentCount { get; set; } = "";

    [Parameter]
    public string StartUrl { get; set; } = "#";

    [Parameter]
    public RenderFragment? IconContent { get; set; }
}
