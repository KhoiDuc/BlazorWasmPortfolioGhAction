using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Events;
using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Pages.SvgDiagramComponents;

public partial class SvgDiagram : IAsyncDisposable
{
    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    [Inject]
    public ComponentBus Bus { get; set; }

    [Parameter]
    public string DiagramSvgId { get; set; } = "diagram";

    private IJSObjectReference? _module;
    private DotNetObjectReference<SvgDiagram>? _dotNetHelper;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Bus.Subscribe<DiagramParametersChangedEvent>(DiagramParametersChangedEventHandler);
        Bus.Subscribe<DiagramAddShapeEvent>(DiagramAddShapeEventHandler);
        Bus.Subscribe<DiagramRemoveSelectedElementsEvent>
            (DiagramRemoveSelectedElementsEventHandler);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                "./js/svg-diagram/svg-diagram.js");
            _dotNetHelper = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("DotNetHelperWrapper.setDotNetHelper",
                _dotNetHelper);

            await CreateDiagramAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        Bus.UnSubscribe<DiagramParametersChangedEvent>(DiagramParametersChangedEventHandler);
        Bus.UnSubscribe<DiagramAddShapeEvent>(DiagramAddShapeEventHandler);
        Bus.UnSubscribe<DiagramRemoveSelectedElementsEvent>
            (DiagramRemoveSelectedElementsEventHandler);

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                //TODO: show error
            }
        }

        _dotNetHelper?.Dispose();
    }

    [JSInvokable]
    public void UpdateSelectedElementsInformation(string[] informationLines)
    {
        Bus.Publish(new DiagramSelectedElementsInformationChangedEvent
        {
            InformationLines = informationLines
        });
    }

    public async Task CreateDiagramAsync()
    {
        var parameters = new DiagramParametersModel();
        await _module!.InvokeVoidAsync("createSvgDiagram", DiagramSvgId,
            parameters.Width, parameters.Height, parameters.ShowGrid);
    }

    private async Task DiagramParametersChangedEventHandler(MessageArgs args,
        CancellationToken ct)
    {
        var message = args.GetMessage<DiagramParametersChangedEvent>();
        if (message is null)
        {
            return;
        }

        await _module!.InvokeVoidAsync("updateSvgDiagramParameters",
            message.Parameters.Width,
            message.Parameters.Height,
            message.Parameters.ShowGrid,
            message.Parameters.GridStep);
    }

    private async Task DiagramAddShapeEventHandler(MessageArgs args,
        CancellationToken ct)
    {
        var message = args.GetMessage<DiagramAddShapeEvent>();
        if (message is null)
        {
            return;
        }

        var addShapeJsFunctionName = message.ShapeType switch
        {
            ShapeType.Rectangle => "addSvgDiagramRectangle",
            ShapeType.Circle => "addSvgDiagramCircle",
            ShapeType.Line => "addSvgDiagramLine",
            _ => string.Empty,
        };

        if (string.IsNullOrWhiteSpace(addShapeJsFunctionName))
        {
            return;
        }

        await _module!.InvokeVoidAsync(addShapeJsFunctionName);
    }

    private async Task DiagramRemoveSelectedElementsEventHandler(MessageArgs args,
        CancellationToken ct)
    {
        var message = args.GetMessage<DiagramRemoveSelectedElementsEvent>();
        if (message is null)
        {
            return;
        }

        await _module!.InvokeVoidAsync("removeSvgDiagramSelectedElements");
    }
}