using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Events;
using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramElementsPanel
{
    [Inject]
    public ComponentBus Bus { get; set; }

    private void OnAddShape(ShapeType shapeType)
    {
        Bus.Publish(new DiagramAddShapeEvent { ShapeType = shapeType });
    }

    private void OnRemoveSelectedElements()
    {
        Bus.Publish(new DiagramRemoveSelectedElementsEvent());
    }
}