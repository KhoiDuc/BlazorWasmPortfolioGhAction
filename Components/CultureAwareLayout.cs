using BlazorWasmPortfolioGhAction.Services.Localization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Components;

/// <summary>
/// Layout base that re-renders body when culture changes.
/// </summary>
public abstract class CultureAwareLayout : LayoutComponentBase, IDisposable
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
