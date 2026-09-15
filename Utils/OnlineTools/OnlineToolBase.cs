using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Utils.OnlineTools;

/// <summary>
/// Base class for online tool components that sync state to URL query parameters.
/// Provides debounced URL updates (500ms) to avoid per-keystroke LocationChanged events
/// that cause route-progress flicker and culture-guard reloads.
/// </summary>
public abstract class OnlineToolBase : ComponentBase, IDisposable
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private System.Threading.Timer? _urlTimer;
    private const int DebounceMs = 500;

    /// <summary>
    /// Schedules a debounced URL update. Rapid calls coalesce — only the last
    /// call within the debounce window executes.
    /// </summary>
    protected void UpdateUrlDebounced(bool replaceHistory = true)
    {
        _urlTimer?.Dispose();
        _urlTimer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(() =>
            {
                NavigationManager.UpdateUrlUsingParameters(this, replaceHistory);
                StateHasChanged();
            });
        }, null, DebounceMs, Timeout.Infinite);
    }

    public virtual void Dispose()
    {
        _urlTimer?.Dispose();
        _urlTimer = null;
        GC.SuppressFinalize(this);
    }
}