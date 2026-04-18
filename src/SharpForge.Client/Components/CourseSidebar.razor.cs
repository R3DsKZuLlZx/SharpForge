using Microsoft.AspNetCore.Components;

namespace SharpForge.Client.Components;

public partial class CourseSidebar
{
    [Parameter, EditorRequired]
    public string CourseId { get; set; } = "";

    [Parameter]
    public int TotalLessons { get; set; }

    [Parameter]
    public string Duration { get; set; } = "";

    [Parameter]
    public int ExerciseCount { get; set; }

    [Parameter]
    public string ProjectDescription { get; set; } = "";

    [Parameter]
    public string StartUrl { get; set; } = "#";

    private int _completedLessons;
    private int[] _completedLessonNumbers = [];
    private bool _hasLoadedProgress;
    private int ProgressPercent => TotalLessons > 0 ? (_completedLessons * 100) / TotalLessons : 0;

    private string ContinueUrl => _completedLessons > 0 && _completedLessons < TotalLessons
        ? $"{StartUrl.TrimEnd('1').TrimEnd('/')}/{GetNextLessonNumber()}"
        : StartUrl;

    private int GetNextLessonNumber()
    {
        // Find the first incomplete lesson
        for (int i = 1; i <= TotalLessons; i++)
        {
            if (!_completedLessonNumbers.Contains(i))
                return i;
        }

        return TotalLessons;
    }

    protected override void OnInitialized()
    {
        ProgressService.OnProgressChanged += HandleProgressChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadProgressAsync();
            _hasLoadedProgress = true;
            StateHasChanged();
        }
    }

    private async void HandleProgressChanged()
    {
        if (_hasLoadedProgress)
        {
            await LoadProgressAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadProgressAsync()
    {
        if (!string.IsNullOrEmpty(CourseId))
        {
            _completedLessonNumbers = await ProgressService.GetCompletedLessonsAsync(CourseId);
            _completedLessons = _completedLessonNumbers.Length;
        }
    }

    public void Dispose()
    {
        ProgressService.OnProgressChanged -= HandleProgressChanged;
    }
}
