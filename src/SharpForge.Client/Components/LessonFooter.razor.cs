using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class LessonFooter
{
    [Parameter]
    public string PreviousUrl { get; set; } = "#";

    [Parameter]
    public string PreviousText { get; set; } = "Previous";

    [Parameter]
    public string NextUrl { get; set; } = "#";

    [Parameter]
    public string NextText { get; set; } = "Next";

    [Parameter]
    public bool ShowHomeIcon { get; set; } = false;

    private async Task HandleNextClickAsync()
    {
        // Parse the current URL to get CourseId and LessonNumber
        // URL format: /courses/{courseId}/lesson/{lessonNumber}
        var uri = new Uri(NavigationManager.Uri);
        var segments = uri.AbsolutePath.Trim('/').Split('/');

        if (segments.Length >= 4 && segments[0] == "courses" && segments[2] == "lesson")
        {
            var courseId = segments[1];
            if (int.TryParse(segments[3], out var lessonNumber))
            {
                await ProgressService.MarkLessonCompleteAsync(courseId, lessonNumber);
            }
        }

        // Navigate to the next lesson/page
        NavigationManager.NavigateTo(NextUrl);
    }
}
