using Fluxor;

namespace BlazorWasmPortfolioGhAction.Store.State
{
    // Stores/DissertationState.cs
    public record DissertationState
    {
        public string ErrorMessage { get; init; } = string.Empty;
        public string CurrentFile { get; init; } = string.Empty;
        public bool IsMobile { get; init; }
        public string FilePath { get; init; }
        public string Title { get; init; }
        public List<string> Images { get; init; }
        public bool FileExists { get; init; }
        public int CurrentImageIndex { get; init; }
        public bool IsFullScreen { get; init; }
        public bool IsLoading { get; init; }
    }

    public class Feature : Feature<DissertationState>
    {
        public override string GetName() => "Dissertation";

        protected override DissertationState GetInitialState() => new()
        {
            IsMobile = false,
            FilePath = string.Empty,
            Title = string.Empty,
            Images = new(),
            FileExists = false,
            CurrentImageIndex = 0,
            IsFullScreen = false,
            IsLoading = true
        };
    }
}
