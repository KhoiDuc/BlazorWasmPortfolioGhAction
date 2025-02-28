using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public interface IMobileDetectionService
    {
        Task<bool> IsMobileDevice();
    }

    // Blazor WebAssembly Implementation
    public class BlazorWebAssemblyMobileDetectionService : IMobileDetectionService
    {
        private readonly IJSRuntime _jsRuntime;

        public BlazorWebAssemblyMobileDetectionService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsMobileDevice()
        {
            var userAgent = await _jsRuntime.InvokeAsync<string>("getUserAgent");
            bool isMobile = IsMobileDevice(userAgent);
            return isMobile;
        }

        private bool IsMobileDevice(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return false;

            var mobileKeywords = new[] { "Android", "webOS", "iPhone", "iPad", "iPod", "BlackBerry", "IEMobile", "Opera Mini" };
            return mobileKeywords.Any(keyword => userAgent.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
