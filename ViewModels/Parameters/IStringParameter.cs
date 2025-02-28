namespace BlazorWasmPortfolioGhAction.ViewModels.Parameters;

public interface IStringParameter
{
    string GetFieldName();

    void SetValue(string? value);

    string GetCaption();
}