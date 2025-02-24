namespace BlazorWasmPortfolioGhAction.Shared.Model;

public class DiagramParametersModel
{
    public int Width { get; set; } = 400;

    public int Height { get; set; } = 300;

    public bool ShowGrid { get; set; }

    public int GridStep { get; set; } = 15;
}