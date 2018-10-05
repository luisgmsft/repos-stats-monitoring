
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using repos_stats.Clients;
using repos_stats.Models;

namespace repos_stats
{
    public static class AzureCatReposStats
    {
        // https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static GitHubClient _githubClient = new GitHubClient();

        [FunctionName("get-stats")]
        public static async Task<IActionResult> GetStats([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stats")]HttpRequest req, ILogger log)
        {
            var repos = await _githubClient.ListRepositories();
            //repos.ForEach(async e => {
            repos.Where(e => e.Permissions.Admin).ToList().ForEach(async e => {
                try
                {
                    var traffic = await _githubClient.ListTrafficViews(log, e.Owner.Login, e.Name);
                    var clones = await _githubClient.ListClones(log, e.Owner.Login, e.Name);
                    var prs = await _githubClient.ListPRs(log, e.Owner.Login, e.Name);
                    log.LogInformation($"Repo: {e.FullName}\nSubscribers: {e.SubscribersCount}\nStargazers: {e.StargazersCount}\nIssues: {e.OpenIssuesCount}\nViews: {traffic.Count}\nUnique Views: {traffic.Uniques}\nUnique Clones: {clones.Uniques}\nPRs: {prs.Count}");
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            });
            return new OkResult();
        }
    }
}
