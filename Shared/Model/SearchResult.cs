using BlazorWasmPortfolioGhAction.Shared.Model;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public class SearchResult
	{
		public ContentHolder Content { get; set; } = null!;
		public int Score { get; set; }
	}
}
