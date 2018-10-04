using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using repos_stats.Models;

namespace repos_stats.Clients
{
    public class GitHubClient
    {
        private readonly HttpClient _client;

        public GitHubClient()
        {
            var pat = Environment.GetEnvironmentVariable("GitHub_PAT");
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.github.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", pat);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("MSPNP Repos Stats Client v.1.0.0");
        }

        public async Task<List<Repository>> ListRepositories()
        {
            var rawRepos = await _client.GetAsync("/user/repos");
            var payload = await rawRepos.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Repository>>(payload);
        }

        public async Task<RepositoryTrafficView> ListTrafficViews(ILogger log, string owner, string repo)
        {
            try
            {
                var rawTrafficViews = await _client.GetAsync($"/repos/{owner}/{repo}/traffic/views");
                var payload = await rawTrafficViews.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RepositoryTrafficView>(payload);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw ex;
            }
        }

        public async Task<RepositoryTrafficClone> ListClones(ILogger log, string owner, string repo)
        {
            try
            {
                var rawTrafficViews = await _client.GetAsync($"/repos/{owner}/{repo}/traffic/clones");
                var payload = await rawTrafficViews.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RepositoryTrafficClone>(payload);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw ex;
            }
        }
    }
}