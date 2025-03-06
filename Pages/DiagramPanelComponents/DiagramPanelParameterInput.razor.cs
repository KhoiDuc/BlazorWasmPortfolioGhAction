using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramPanelParameterInput
{
    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string InputHtmlType { get; set; } = string.Empty;

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<ChangeEventArgs> OnValueChanged { get; set; }

    [Parameter]
    public Expression<Func<string?>> ValudationMessgeFor { get; set; } = () => string.Empty;
}