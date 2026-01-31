using Microsoft.JSInterop;

namespace SharpForge.Blazor.Client.Services;

public class ThemeService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private bool _isDarkMode = true;

    public event Action? OnThemeChanged;

    public bool IsDarkMode => _isDarkMode;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
        var savedTheme = await _module.InvokeAsync<string?>("getTheme");
        
        if (savedTheme != null)
        {
            _isDarkMode = savedTheme == "dark";
        }
        else
        {
            // Check system preference
            _isDarkMode = await _module.InvokeAsync<bool>("getSystemPreference");
        }

        await ApplyThemeAsync();
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(bool isDarkMode)
    {
        _isDarkMode = isDarkMode;
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("setTheme", _isDarkMode ? "dark" : "light");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}

