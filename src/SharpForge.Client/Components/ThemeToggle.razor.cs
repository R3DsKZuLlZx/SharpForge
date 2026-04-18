namespace SharpForge.Client.Components;

public partial class ThemeToggle
{
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += StateHasChanged;
    }

    private async Task ToggleTheme()
    {
        await ThemeService.ToggleThemeAsync();
    }

    public void Dispose()
    {
        ThemeService.OnThemeChanged -= StateHasChanged;
    }
}
