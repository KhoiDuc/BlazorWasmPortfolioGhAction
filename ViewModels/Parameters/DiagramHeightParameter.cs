namespace BlazorWasmPortfolioGhAction.ViewModels.Parameters;

public class DiagramHeightParameter : IStringParameter
{
    private readonly DiagramParametersViewModel _parametersViewModel;

    public DiagramHeightParameter(DiagramParametersViewModel parametersViewModel) =>
        _parametersViewModel = parametersViewModel;

    public string GetFieldName() => nameof(_parametersViewModel.Height);

    public void SetValue(string? value) => _parametersViewModel.Height = value;

    public string GetCaption() => DiagramParametersViewModel.HeightCaption;
}