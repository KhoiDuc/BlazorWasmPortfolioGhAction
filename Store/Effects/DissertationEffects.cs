using BlazorWasmPortfolioGhAction.Store.Services;
using Fluxor;
using static BlazorWasmPortfolioGhAction.Store.Actions.DissertationAction;

namespace BlazorWasmPortfolioGhAction.Store.Effects
{
    public class DissertationEffects
    {
        private readonly HttpClient _http;
        private readonly IMobileDetectionService _mobileService;

        public DissertationEffects(HttpClient http, IMobileDetectionService mobileService)
        {
            _http = http;
            _mobileService = mobileService;
        }

        [EffectMethod]
        public async Task HandleInitialize(InitializeAction action, IDispatcher dispatcher)
        {
            try
            {
                dispatcher.Dispatch(new SetLoadingAction(true));
                dispatcher.Dispatch(new ClearContentAction());

                var (path, title, images) = GetFileData(action.File);

                if (string.IsNullOrEmpty(path))
                {
                    dispatcher.Dispatch(new FileErrorAction("Invalid file request"));
                    return;
                }

                dispatcher.Dispatch(new SetFileDataAction(path, title, images));

                var isMobile = await _mobileService.IsMobileDevice();
                dispatcher.Dispatch(new SetDeviceTypeAction(isMobile));

                // Mobile-specific logic
                if (isMobile)
                {
                    // Mobile doesn't need file existence check
                    dispatcher.Dispatch(new SetLoadingAction(false));
                    return;
                }

                // Desktop-specific logic
                try
                {
                    var response = await _http.GetAsync(path);
                    dispatcher.Dispatch(new FileExistsResultAction(response.IsSuccessStatusCode));
                }
                catch
                {
                    dispatcher.Dispatch(new FileExistsResultAction(false));
                }
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new FileErrorAction($"Error loading document: {ex.Message}"));
            }
        }

        [EffectMethod]
        public async Task HandleCheckFileExists(CheckFileExistsAction action, IDispatcher dispatcher)
        {
            try
            {
                var response = await _http.GetAsync(action.FilePath);
                dispatcher.Dispatch(new FileExistsResultAction(response.IsSuccessStatusCode));
            }
            catch
            {
                dispatcher.Dispatch(new FileExistsResultAction(false));
            }
        }

        private static (string path, string title, List<string> images) GetFileData(string file) => file switch
        {
            "Transcript" => ("Transcript.pdf", "My transcript", new() { "transcript1.png" }),
            "Degree" => ("Degree.pdf", "My bachelor's degree", new() { "degree1.png", "degree2.png", "degree3.png" }),
            "CV" => ("khoicv.doc.pdf", "My curriculum vitae", new() { "khoicv_Page1.png", "khoicv_Page2.png", "khoicv_Page3.png" }),
            _ => (string.Empty, string.Empty, new())
        };
    }
}
