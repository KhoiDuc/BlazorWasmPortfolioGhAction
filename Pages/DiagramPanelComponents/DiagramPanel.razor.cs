using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramPanel
{
    [Parameter]
    public DiagramParametersModel DiagramParametersModel { get; set; } = new();

    [Parameter]
    public string[] DiagramInformationLinesModel { get; set; } = [];
}