using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class LessonSidebar
{
    [Parameter]
    public string CourseUrl { get; set; } = "#";

    [Parameter]
    public string CourseTitle { get; set; } = "";

    [Parameter]
    public int CurrentLesson { get; set; } = 1;

    [Parameter]
    public List<CourseLessonDetail> Lessons { get; set; } = [];

    private bool _isOpen = false;

    private void ToggleSidebar()
    {
        _isOpen = !_isOpen;
    }

    private void CloseSidebar()
    {
        _isOpen = false;
    }
}
