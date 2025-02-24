using BlazorWasmPortfolioGhAction.ViewModels;

namespace BlazorWasmPortfolioGhAction.ViewModels.Parameters;

public class DiagramGridStepParameter : IStringParameter
{
    private readonly DiagramParametersViewModel _parametersViewModel;

    public DiagramGridStepParameter(DiagramParametersViewModel parametersViewModel) =>
        _parametersViewModel = parametersViewModel;

    public string GetFieldName() => nameof(_parametersViewModel.GridStep);

    public void SetValue(string? value) => _parametersViewModel.GridStep = value;

    public string GetCaption() => DiagramParametersViewModel.GridStepCaption;
}