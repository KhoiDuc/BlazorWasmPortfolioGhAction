using BlazorWasmPortfolioGhAction.Store.Services;

namespace BlazorWasmPortfolioGhAction.Services;

public class StateContainer
{
    public List<GitHubUser> SearchedUsers { get; } = new();
}
