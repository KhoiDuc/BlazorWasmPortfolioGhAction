using BlazorWasmPortfolioGhAction.Services;
using BlazorWasmPortfolioGhAction.Store.Actions;
using BlazorWasmPortfolioGhAction.Store.State;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Store.Effects;

public class ContentEffects
{
    private readonly IWikiContentService _wikiContentService;
    private readonly ILogger<ContentEffects> _logger;

    public ContentEffects(IWikiContentService wikiContentService, ILogger<ContentEffects> logger)
    {
        _wikiContentService = wikiContentService;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadContentsFromRepo(LoadContentsFromRepoAction action, IDispatcher dispatcher)
    {
        try
        {
            var files = await _wikiContentService.GetManifestFilesAsync();
            dispatcher.Dispatch(new LoadContentsFromRepoSuccessAction(files.Count));

            foreach (var file in files)
            {
                dispatcher.Dispatch(new FetchFileContentsAction($"wiki/{file}.json", file));
            }

            if (files.Count == 0)
            {
                dispatcher.Dispatch(new AllFilesFetchedAction());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load wiki manifest");
            dispatcher.Dispatch(new LoadContentsFromRepoSuccessAction(0));
        }
    }

    [EffectMethod]
    public async Task HandleFetchFileContents(FetchFileContentsAction action, IDispatcher dispatcher)
    {
        try
        {
            var contents = await _wikiContentService.LoadFileContentsAsync(action.FileNameWithoutSuffix);
            dispatcher.Dispatch(new FileContentFetchedAction(action.FileNameWithoutSuffix, contents));

            var sha = await _wikiContentService.GetFileShaAsync(action.FileNameWithoutSuffix);
            if (!string.IsNullOrEmpty(sha))
                dispatcher.Dispatch(new UpdateShaDictionaryAction(action.FileNameWithoutSuffix, sha));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch wiki file {File}", action.FileNameWithoutSuffix);
            dispatcher.Dispatch(new FileContentFetchedAction(action.FileNameWithoutSuffix, []));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateGitHubContent(UpdateGitHubContentAction action, IDispatcher dispatcher)
    {
        try
        {
            var success = await _wikiContentService.UpdateGitHubContentAsync(
                action.ContentHolders,
                action.CommitMessage,
                action.Page,
                action.Section,
                action.ShaDictionary);

            if (!success)
            {
                _logger.LogWarning("Wiki content update failed — check DevOps:GitHubToken and Wiki config");
                return;
            }

            var fileKey = $"{action.Page}{action.Section}";
            var sha = await _wikiContentService.GetFileShaAsync(fileKey);
            if (!string.IsNullOrEmpty(sha))
                dispatcher.Dispatch(new UpdateShaDictionaryAction(fileKey, sha));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wiki content on GitHub");
        }
    }

    [EffectMethod]
    public async Task HandleDeleteFileOnGithub(DeleteFileOnGithubAction action, IDispatcher dispatcher)
    {
        try
        {
            await _wikiContentService.DeleteGitHubFileAsync(action.CommitMessage, action.Section, action.Sha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete wiki file on GitHub");
        }
    }
}
