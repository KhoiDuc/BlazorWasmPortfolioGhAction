using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IClipboardService
{
    Task CopyTextAsync(string text);
}

public class ClipboardService : IClipboardService
{
    private readonly IJSRuntime _js;

    public ClipboardService(IJSRuntime js) => _js = js;

    public Task CopyTextAsync(string text) =>
        _js.InvokeVoidAsync("navigator.clipboard.writeText", text).AsTask();
}
