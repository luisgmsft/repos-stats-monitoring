
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
    public class RepoStat
    {
        public RepoStat()
        {
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; private set; }
        public string Repo { get; set; }
        public int Subscribers { get; set; }
        public int Stargazers { get; set; }
        public int Issues { get; set; }
        public int Views { get; set; }
        public int UniqueViews { get; set; }
        public int UniqueClones { get; set; }
        public int PRs { get; set; }
    }

    public static class AzureCatReposStats
    {
        // https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static GitHubClient _githubClient = new GitHubClient();

        [FunctionName("cron-stats")]
        public static async Task CronStats(
            // https://codehollow.com/2017/02/azure-functions-time-trigger-cron-cheat-sheet/
            [TimerTrigger("0 0 1 * * *")]TimerInfo myTimer
            , [CosmosDB(
                databaseName: "repos-stats-sql-database",
                collectionName: "repos-stats-sql-collection",
                ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<RepoStat> statsOut
            , ILogger log)
        {
            var repos = await _githubClient.ListRepositories();
            //repos.ForEach(async e => {
            await Task.WhenAll(repos
                .Where(e => e.Permissions.Admin)
                .Select(async e => {
                try
                {
                    var traffic = await _githubClient.ListTrafficViews(log, e.Owner.Login, e.Name);
                    var clones = await _githubClient.ListClones(log, e.Owner.Login, e.Name);
                    var prs = await _githubClient.ListPRs(log, e.Owner.Login, e.Name);
                    await statsOut.AddAsync(new RepoStat {
                        Repo = e.FullName,
                        Subscribers = e.SubscribersCount,
                        Stargazers = e.StargazersCount,
                        Issues = e.OpenIssuesCount,
                        Views = traffic.Count,
                        UniqueViews = traffic.Uniques,
                        UniqueClones = clones.Uniques,
                        PRs = prs.Count
                    });
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            }));
        }
    }
}
