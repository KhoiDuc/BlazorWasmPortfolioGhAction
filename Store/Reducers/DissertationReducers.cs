using BlazorWasmPortfolioGhAction.Store.State;
using Fluxor;
using static BlazorWasmPortfolioGhAction.Store.Actions.DissertationAction;

namespace BlazorWasmPortfolioGhAction.Store.Reducers
{
    public static class DissertationReducers
    {
        [ReducerMethod]
        public static DissertationState ReduceSetLoading(DissertationState state, SetLoadingAction action) =>
    state with { IsLoading = action.Loading };

        [ReducerMethod]
        public static DissertationState ReduceFileExistsResult(DissertationState state, FileExistsResultAction action) =>
            state with
            {
                FileExists = action.Exists,
                IsLoading = false
            };
        [ReducerMethod]
        public static DissertationState ReduceFileError(DissertationState state, FileErrorAction action) =>
            state with
            {
                ErrorMessage = action.Message,
                IsLoading = false
            };
        [ReducerMethod]
        public static DissertationState ReduceClearContent(
    DissertationState state,
    ClearContentAction _) =>
    state with
    {
        FilePath = string.Empty,
        Title = string.Empty,
        Images = new List<string>(),
        FileExists = false
    };

        [ReducerMethod]
        public static DissertationState ReduceSetFileData(
            DissertationState state,
            SetFileDataAction action) =>
            state with
            {
                FilePath = action.FilePath,
                Title = action.Title,
                Images = action.Images
            };
        [ReducerMethod]
        public static DissertationState ReduceInitializeAction(
            DissertationState state,
            InitializeAction action) =>
            state with
            {
                CurrentFile = action.File,
                IsLoading = true,
                FilePath = string.Empty,
                Title = string.Empty,
                Images = new List<string>()
            };

        [ReducerMethod]
        public static DissertationState ReduceSetDeviceType(DissertationState state, SetDeviceTypeAction action) =>
            state with { IsMobile = action.IsMobile };

        [ReducerMethod]
        public static DissertationState ReduceFileExists(DissertationState state, FileExistsResultAction action) =>
            state with { FileExists = action.Exists, IsLoading = false };

        [ReducerMethod]
        public static DissertationState ReduceOpenFullImage(DissertationState state, OpenFullImageAction action) =>
            state with { CurrentImageIndex = action.Index, IsFullScreen = true };

        [ReducerMethod]
        public static DissertationState ReduceCloseFullImage(DissertationState state, CloseFullImageAction _) =>
            state with { IsFullScreen = false };
    }
}
