namespace BlazorWasmPortfolioGhAction.Shared.Model
{
	public class PUTContentParser
	{
		public Content content { get; set; } = null!;
		public Commit commit { get; set; } = null!;
	}
	public class Content
	{
		public string sha { get; set; } = string.Empty;
	}
	public class Commit
	{
		public string sha { get; set; } = string.Empty;
	}
}
