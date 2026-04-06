using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;
using SharpForge.Client.Services;

namespace SharpForge.Client.Pages;

public partial class Course
{
    [Inject]
    public required MarkdownService MarkdownService { get; set; }
    
    [Parameter]
    public string Slug { get; set; } = "";

    private CourseDetail? _course;

    protected override void OnParametersSet()
    {
        _course = MarkdownService.GetCourse(Slug);
    }
}
