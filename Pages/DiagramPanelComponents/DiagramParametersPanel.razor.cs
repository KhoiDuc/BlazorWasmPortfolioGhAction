using AutoMapper;
using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Events;
using BlazorWasmPortfolioGhAction.Shared.Model;
using BlazorWasmPortfolioGhAction.ViewModels;
using BlazorWasmPortfolioGhAction.ViewModels.Parameters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramParametersPanel
{
    [Inject]
    public ComponentBus Bus { get; set; }

    [Inject]
    public IMapper Mapper { get; set; }

    [Parameter]
    public DiagramParametersModel ParametersModel { get; set; } = new();

    private EditContext? _editContext;
    private ValidationMessageStore? _validationMessagesStore;
    private DiagramParametersViewModel _parametersViewModel = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _parametersViewModel = Mapper.Map<DiagramParametersViewModel>(ParametersModel);
        _editContext = new(_parametersViewModel);
        _validationMessagesStore = new(_editContext);
    }

    public void ChangeParameters()
    {
        var parametersModel = Mapper.Map<DiagramParametersModel>(_parametersViewModel);
        Bus.Publish(new DiagramParametersChangedEvent { Parameters = parametersModel });
    }

    private void OnWidthChanged(ChangeEventArgs args) =>
        OnStringParameterChanged(args, new DiagramWidthParameter(_parametersViewModel));

    private void OnHeightChanged(ChangeEventArgs args) =>
        OnStringParameterChanged(args, new DiagramHeightParameter(_parametersViewModel));

    private void OnShowGridChanged(ChangeEventArgs args)
    {
        _parametersViewModel.ShowGrid = args.Value is not null && (bool)args.Value;
        ChangeParameters();
    }

    private void OnGridStepChanged(ChangeEventArgs args) =>
        OnStringParameterChanged(args, new DiagramGridStepParameter(_parametersViewModel));

    private void OnStringParameterChanged(ChangeEventArgs args, IStringParameter parameter)
    {
        if (_editContext == null || _validationMessagesStore == null)
        {
            return;
        }

        var fieldIdentifier = _editContext.Field(parameter.GetFieldName());
        _validationMessagesStore.Clear(fieldIdentifier);

        parameter.SetValue(args.Value?.ToString());
        try
        {
            _editContext.Validate();
        }
        catch (OverflowException)
        {
            _validationMessagesStore.Add(fieldIdentifier, parameter.GetCaption()
                + " causes an overflow");
            return;
        }

        var fieldIsValid = _editContext.IsValid(fieldIdentifier);
        if (fieldIsValid)
        {
            ChangeParameters();
        }
    }
}