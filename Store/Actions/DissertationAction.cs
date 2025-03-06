namespace BlazorWasmPortfolioGhAction.Store.Actions
{
    public class DissertationAction
    {
        public record ClearContentAction;
        public record SetFileDataAction(string FilePath, string Title, List<string> Images);
        public record FileErrorAction(string Message);
        public record InitializeAction(string File);
        public record SetDeviceTypeAction(bool IsMobile);
        public record CheckFileExistsAction(string FilePath);
        public record FileExistsResultAction(bool Exists);
        public record OpenFullImageAction(int Index);
        public record CloseFullImageAction;
        public record SetLoadingAction(bool Loading);
    }
}
