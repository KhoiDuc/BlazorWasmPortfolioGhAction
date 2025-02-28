using BlazorWasmPortfolioGhAction.Shared.Model;
using Fluxor;
using System.Collections.Generic;

namespace BlazorWasmPortfolioGhAction.Store.State
{
    public record ContentState
	{
		public List<ContentHolder> Contents { get; init; }
		public List<bool> IsEditing { get; init; }
		public int FileCount { get; init; }
		public int LoadedFilesCount { get; init; }
		public Dictionary<string, string> ShaDictionary { get; init; }
		public ContentState()
		{
			Contents = new List<ContentHolder>();
			ShaDictionary = new Dictionary<string, string>();
			IsEditing = new List<bool>();
			FileCount = 0;
			LoadedFilesCount = 0;
		}

		public ContentState(List<ContentHolder> contents, Dictionary<string, string> shaDictionary, List<bool> isEditing, int fileCount, int loadedFilesCount)
		{
			Contents = contents;
			ShaDictionary = shaDictionary;
			IsEditing = isEditing;
			FileCount = fileCount;
			LoadedFilesCount = loadedFilesCount;
		}
	}

	public class ContentFeature : Feature<ContentState>
	{
		public override string GetName() => "Content";

        protected override ContentState GetInitialState()
        {
            var contents = new List<ContentHolder>();
            var isEditing = new List<bool>(new bool[contents.Count]);
            var shaDictionary = new Dictionary<string, string>();
            return new ContentState
            {
                Contents = contents,
                IsEditing = isEditing,
                FileCount = 0,
                LoadedFilesCount = 0,
                ShaDictionary = shaDictionary
            };
        }
    }
}
