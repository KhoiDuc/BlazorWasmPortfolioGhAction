using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IScriptLoaderService
{
    Task EnsureChartJsAsync();
    Task EnsurePhysicsLibsAsync();
    Task EnsureInteractJsAsync();
    Task EnsureTinyMceAsync();
}

public class ScriptLoaderService : IScriptLoaderService
{
    private readonly IJSRuntime _js;

    public ScriptLoaderService(IJSRuntime js) => _js = js;

    public Task EnsureChartJsAsync() =>
        _js.InvokeVoidAsync("scriptLoader.loadChartJs").AsTask();

    public Task EnsurePhysicsLibsAsync() =>
        _js.InvokeVoidAsync("scriptLoader.loadPhysicsLibs").AsTask();

    public Task EnsureInteractJsAsync() =>
        _js.InvokeVoidAsync("scriptLoader.loadInteractJs").AsTask();

    public Task EnsureTinyMceAsync() =>
        _js.InvokeVoidAsync("tinyMceLoader.ensureLoaded").AsTask();
}
