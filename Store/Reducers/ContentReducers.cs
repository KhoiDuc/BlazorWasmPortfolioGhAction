using BlazorWasmPortfolioGhAction.Store.Actions;
using BlazorWasmPortfolioGhAction.Store.State;
using Fluxor;
using BlazorWasmPortfolioGhAction.Shared.Model;

namespace BlazorWasmPortfolioGhAction.Store.Reducers
{
    public static class ContentReducers
    {
        [ReducerMethod]
        public static ContentState ReduceLoadContentsFromRepoSuccessAction(ContentState state, LoadContentsFromRepoSuccessAction action)
        {
            return state with { FileCount = action.FileCount };
        }

        [ReducerMethod]
        public static ContentState ReduceFileContentFetchedAction(ContentState state, FileContentFetchedAction action)
        {
            var updatedContents = new List<ContentHolder>(state.Contents);
            var updatedIsEditing = new List<bool>(state.IsEditing);

            int startId = updatedContents.Count;
            for (int i = 0; i < action.FileContents.Count; i++)
            {
                action.FileContents[i].Id = startId + i;
                updatedContents.Add(action.FileContents[i]);
                updatedIsEditing.Add(false);
            }

            return state with { Contents = updatedContents, IsEditing = updatedIsEditing, LoadedFilesCount = state.LoadedFilesCount + 1 };
        }

        [ReducerMethod]
        public static ContentState ReduceUpdateIsEditingAction(ContentState state, UpdateIsEditingAction action)
        {
            var updatedIsEditing = new List<bool>(state.IsEditing);
            if (action.Index >= 0 && action.Index < updatedIsEditing.Count)
            {
                updatedIsEditing[action.Index] = action.IsEditing;
            }

            return new ContentState(state.Contents, state.ShaDictionary, updatedIsEditing, state.FileCount, state.LoadedFilesCount);
        }

        [ReducerMethod]
        public static ContentState ReduceResetIsEditingAction(ContentState state, ResetIsEditingAction action)
        {
            var updatedIsEditing = new List<bool>(state.IsEditing.Count);
            for (int i = 0; i < state.IsEditing.Count; i++)
            {
                updatedIsEditing.Add(false);
            }
            return state with { IsEditing = updatedIsEditing };
        }

        [ReducerMethod]
        public static ContentState ReduceSaveContentAction(ContentState state, SaveContentAction action)
        {
            var updatedContents = new List<ContentHolder>(state.Contents);

            var index = updatedContents.FindIndex(x => x.Id == action.Index);
            if (index == -1 && action.Index >= 0 && action.Index < updatedContents.Count)
                index = action.Index;

            if (index != -1)
                updatedContents[index] = action.TempContent;
            return state with { Contents = updatedContents };
        }

        [ReducerMethod]
        public static ContentState ReduceUpdateShaDictionaryAction(ContentState state, UpdateShaDictionaryAction action)
        {
            var updatedShaDictionary = state.ShaDictionary is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(state.ShaDictionary);

            updatedShaDictionary[action.Section] = action.Sha;

            return state with { ShaDictionary = updatedShaDictionary };
        }
    }
}