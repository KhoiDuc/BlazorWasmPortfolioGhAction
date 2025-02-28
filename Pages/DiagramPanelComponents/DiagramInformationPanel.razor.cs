using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Events;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Pages.DiagramPanelComponents;

public partial class DiagramInformationPanel : IDisposable
{
    [Inject]
    public ComponentBus Bus { get; set; }

    [Parameter]
    public string[] InformationLines { get; set; } = [];

    private string[] _informationLines = [];

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Bus.Subscribe<DiagramSelectedElementsInformationChangedEvent>
            (DiagramSelectedElementsInformationChangedEventHandler);
    }

    public void Dispose()
    {
        Bus.UnSubscribe<DiagramSelectedElementsInformationChangedEvent>
            (DiagramSelectedElementsInformationChangedEventHandler);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _informationLines = InformationLines;
    }

    private async Task DiagramSelectedElementsInformationChangedEventHandler
        (MessageArgs args, CancellationToken ct)
    {
        var message = args.GetMessage<DiagramSelectedElementsInformationChangedEvent>();
        if (message is null)
        {
            return;
        }

        _informationLines = message.InformationLines;
        await InvokeAsync(StateHasChanged);
    }
}