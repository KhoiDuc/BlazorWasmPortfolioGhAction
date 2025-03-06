using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramPanelParameterCheckbox
{
    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<ChangeEventArgs> OnValueChanged { get; set; }
}