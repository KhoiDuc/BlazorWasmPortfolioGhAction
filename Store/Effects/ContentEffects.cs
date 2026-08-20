using BlazorWasmPortfolioGhAction.Shared.Model;
using BlazorWasmPortfolioGhAction.Store.State;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Store.Effects
{
    public class ContentEffects
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentEffects> _logger;
        private readonly IState<ContentState> _contentState;

        public ContentEffects(HttpClient httpClient, ILogger<ContentEffects> logger, IState<ContentState> contentState)
        {
            _httpClient = httpClient;
            _logger = logger;
            _contentState = contentState;
        }     
    }
}
