using BlazorWasmPortfolioGhAction.Shared.Model;

namespace BlazorWasmPortfolioGhAction.Events;

public class DiagramParametersChangedEvent
{
    public DiagramParametersModel Parameters { get; set; } = new();
}