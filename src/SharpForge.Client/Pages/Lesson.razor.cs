using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;
using SharpForge.Client.Services;

namespace SharpForge.Client.Pages;

public partial class Lesson
{
    [Inject]
    public required MarkdownService MarkdownService { get; set; }
    
    [Parameter]
    public string CourseSlug { get; set; } = "";

    [Parameter]
    public int LessonNumber { get; set; }

    private LessonContent? _lesson;

    protected override void OnParametersSet()
    {
        _lesson = MarkdownService.GetLesson(CourseSlug, LessonNumber);
    }
}
