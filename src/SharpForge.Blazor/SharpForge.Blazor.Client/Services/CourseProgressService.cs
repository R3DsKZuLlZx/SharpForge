using Microsoft.JSInterop;

namespace SharpForge.Blazor.Client.Services;

public class CourseProgressService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public event Action? OnProgressChanged;

    public CourseProgressService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task EnsureModuleLoadedAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/courseProgress.js");
    }

    public async Task<int[]> GetCompletedLessonsAsync(string courseId)
    {
        await EnsureModuleLoadedAsync();
        return await _module!.InvokeAsync<int[]>("getCompletedLessons", courseId);
    }

    public async Task<int> GetCompletedLessonCountAsync(string courseId)
    {
        await EnsureModuleLoadedAsync();
        return await _module!.InvokeAsync<int>("getCompletedLessonCount", courseId);
    }

    public async Task<bool> IsLessonCompleteAsync(string courseId, int lessonNumber)
    {
        await EnsureModuleLoadedAsync();
        return await _module!.InvokeAsync<bool>("isLessonComplete", courseId, lessonNumber);
    }

    public async Task MarkLessonCompleteAsync(string courseId, int lessonNumber)
    {
        await EnsureModuleLoadedAsync();
        await _module!.InvokeVoidAsync("markLessonComplete", courseId, lessonNumber);
        OnProgressChanged?.Invoke();
    }

    public async Task MarkLessonIncompleteAsync(string courseId, int lessonNumber)
    {
        await EnsureModuleLoadedAsync();
        await _module!.InvokeVoidAsync("markLessonIncomplete", courseId, lessonNumber);
        OnProgressChanged?.Invoke();
    }

    public async Task ResetCourseProgressAsync(string courseId)
    {
        await EnsureModuleLoadedAsync();
        await _module!.InvokeVoidAsync("resetCourseProgress", courseId);
        OnProgressChanged?.Invoke();
    }

    public async Task ResetAllProgressAsync()
    {
        await EnsureModuleLoadedAsync();
        await _module!.InvokeVoidAsync("resetAllProgress");
        OnProgressChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}

