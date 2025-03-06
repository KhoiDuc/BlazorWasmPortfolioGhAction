using BlazorWasmPortfolioGhAction.Shared.Model;
using BlazorWasmPortfolioGhAction.Store.Actions;
using BlazorWasmPortfolioGhAction.Store.State;
using Fluxor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

