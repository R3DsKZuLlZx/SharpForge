using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class LessonLayout
{
    /// <summary>The course slug, e.g. "aspnet-core-web-apis".</summary>
    [Parameter, EditorRequired]
    public required string CourseSlug { get; set; }

    /// <summary>The 1-based lesson number within the course.</summary>
    [Parameter, EditorRequired]
    public int LessonNumber { get; set; }

    /// <summary>Estimated reading/watching duration, e.g. "8 min".</summary>
    [Parameter]
    public string Duration { get; set; } = "10 min";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string CourseUrl => $"courses/{CourseSlug}";
    private CourseDetail _course = null!;
    private CourseLessonDetail _lesson = null!;
    private string _previousUrl = "#";
    private string _previousText = "Previous Lesson";
    private string _nextUrl = "#";
    private string _nextText = "Next Lesson";
    private bool _showHomeIcon;

    protected override void OnParametersSet()
    {
        _course = MarkdownService.GetCourse(CourseSlug)
                  ?? throw new InvalidOperationException($"Course '{CourseSlug}' not found.");
        _lesson = _course.Lessons.First(l => l.Number == LessonNumber);

        var isFirst = LessonNumber == _course.Lessons[0].Number;
        var isLast = LessonNumber == _course.Lessons[^1].Number;

        if (isFirst)
        {
            _previousUrl = CourseUrl;
            _previousText = "Course Overview";
        }
        else
        {
            _previousUrl = _course.GetLessonUrl(LessonNumber - 1);
            _previousText = "Previous Lesson";
        }

        if (isLast)
        {
            _nextUrl = CourseUrl;
            _nextText = "Back to Course";
            _showHomeIcon = true;
        }
        else
        {
            _nextUrl = _course.GetLessonUrl(LessonNumber + 1);
            _nextText = "Next Lesson";
            _showHomeIcon = false;
        }
    }
}
