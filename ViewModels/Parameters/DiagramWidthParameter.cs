using BlazorWasmPortfolioGhAction.ViewModels;

namespace BlazorWasmPortfolioGhAction.ViewModels.Parameters;

public class DiagramWidthParameter : IStringParameter
{
    private readonly DiagramParametersViewModel _parametersViewModel;

    public DiagramWidthParameter(DiagramParametersViewModel parametersViewModel) =>
        _parametersViewModel = parametersViewModel;

    public string GetFieldName() => nameof(_parametersViewModel.Width);

    public void SetValue(string? value) => _parametersViewModel.Width = value;

    public string GetCaption() => DiagramParametersViewModel.WidthCaption;
}