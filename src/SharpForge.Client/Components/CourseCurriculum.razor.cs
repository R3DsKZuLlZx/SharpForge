using Microsoft.AspNetCore.Components;
using SharpForge.Client.Models;

namespace SharpForge.Client.Components;

public partial class CourseCurriculum
{
    [Parameter]
    public string BaseUrl { get; set; } = "";

    [Parameter]
    public List<CourseLessonDetail> Lessons { get; set; } = [];
}
