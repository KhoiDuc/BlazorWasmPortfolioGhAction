﻿using BlazorWasmPortfolioGhAction.Shared.Model;
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
        [EffectMethod]
        public async Task HandleLoadContentsFromRepoAction(LoadContentsFromRepoAction action, IDispatcher dispatcher)
        {
            //_logger.LogInformation("Loading contents from repo...");

            try
            {
                var response = await _httpClient.GetAsync("https://samdenton.tech/GithubGetAll-proxy.php");
                var content = await response.Content.ReadAsStringAsync();
                //_logger.LogInformation("contents from repo..." + content);
                //var bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                //if (content.StartsWith(bom))
                //{
                //    content = content.Remove(0, bom.Length);
                //}

                var gitContents = JsonConvert.DeserializeObject<List<ContentParser>>(content);
                var fileCount = 0;

                foreach (var gitContent in gitContents)
                {
                    if (gitContent.type == "file")
                    {
                        string fileNameWithoutSuffix = gitContent.name.Replace("_data.json", "");
                        dispatcher.Dispatch(new FetchFileContentsAction(gitContent.url, fileNameWithoutSuffix));
                        fileCount++;
                    }
                }

                dispatcher.Dispatch(new LoadContentsFromRepoSuccessAction(fileCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load contents from repo");
            }
        }

        [EffectMethod]
        public async Task HandleFetchFileContentsAction(FetchFileContentsAction action, IDispatcher dispatcher)
        {
            //Console.WriteLine($"Fetching file content: {action.FileNameWithoutSuffix}...");

            try
            {
                var fileResponse = await _httpClient.GetAsync($"https://samdenton.tech/GithubGetFile-proxy.php?url={WebUtility.UrlEncode(action.FileUrl)}");
                var fileContent = await fileResponse.Content.ReadAsStringAsync();

                // Remove potential Byte Order Mark (BOM)
                //var bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                //if (fileContent.StartsWith(bom))
                //{
                //    fileContent = fileContent.Remove(0, bom.Length);
                //}

                var fileGitContent = JsonConvert.DeserializeObject<ContentParser>(fileContent);

                var contentBytes = Convert.FromBase64String(fileGitContent.content);
                var jsonContent = Encoding.UTF8.GetString(contentBytes);
                var array = JsonConvert.DeserializeObject<List<ContentHolder>>(jsonContent);
				dispatcher.Dispatch(new FileContentFetchedAction(action.FileNameWithoutSuffix, array));

                var newSha = fileGitContent.sha;
				dispatcher.Dispatch(new UpdateShaDictionaryAction(action.FileNameWithoutSuffix, newSha));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch file content: {action.FileNameWithoutSuffix}");
            }
        }
		[EffectMethod]
		public async Task HandleUpdateGitHubContentAction(UpdateGitHubContentAction action, IDispatcher dispatcher)
		{
            //try
            //{
                var filename = action.Page + action.Section;
				Console.WriteLine("PUT'ing: " + filename);
				var jsonString = JsonConvert.SerializeObject(action.ContentHolders);
				var Sha = _contentState.Value.ShaDictionary[filename];

				var body = new
				{
					message = action.CommitMessage,
					content = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString)),
					sha = Sha
				};

				var json = JsonConvert.SerializeObject(body);
				var contentAndMessage = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PutAsync($"https://samdenton.tech/GithubPut-proxy.php?section={filename}", contentAndMessage);
				var responseBytes = await response.Content.ReadAsByteArrayAsync();
				var bom = Encoding.UTF8.GetPreamble();
				if (responseBytes.Take(bom.Length).SequenceEqual(bom))
				{
					responseBytes = responseBytes.Skip(bom.Length).ToArray();
				}

				var responceHeaders = Encoding.UTF8.GetString(responseBytes);
				var gitContent = JsonConvert.DeserializeObject<PUTContentParser>(responceHeaders);
				var newSha = gitContent.content.sha;
				dispatcher.Dispatch(new UpdateShaDictionaryAction(filename, newSha));

				response.EnsureSuccessStatusCode();
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception("Error updating content: " + response.ReasonPhrase);
				}
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: " + ex.Message);
            //}
        }
		[EffectMethod]
		public async Task HandleDeleteFileOnGithubAction(DeleteFileOnGithubAction action, IDispatcher dispatcher)
		{
			// Your HTTP DELETE request logic here
		}
	}
}

