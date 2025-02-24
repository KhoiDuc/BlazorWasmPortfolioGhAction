using BlazorWasmPortfolioGhAction.Shared.Model;

namespace BlazorWasmPortfolioGhAction.Events;

public class DiagramAddShapeEvent
{
    public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;
}