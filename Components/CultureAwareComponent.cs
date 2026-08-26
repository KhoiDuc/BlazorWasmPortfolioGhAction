using BlazorWasmPortfolioGhAction.Services.Localization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Components;

/// <summary>
/// Subscribes to culture changes so localized UI re-renders after Navbar toggle.
/// </summary>
public abstract class CultureAwareComponent : ComponentBase, IDisposable
{
    [Inject] protected ICultureService CultureService { get; set; } = default!;

    protected override void OnInitialized()
    {
        CultureService.CultureChanged += OnCultureChanged;
    }

    private void OnCultureChanged() => InvokeAsync(StateHasChanged);

    public virtual void Dispose()
    {
        CultureService.CultureChanged -= OnCultureChanged;
    }
}
